using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class RequestService
    {
        private readonly IRequeteRepository _requeteRepo;
        private readonly AppDbContext _db;
        private const string UploadsDir = "wwwroot/uploads/requests";

        public RequestService(IRequeteRepository requeteRepo, AppDbContext db)
        {
            _requeteRepo = requeteRepo;
            _db = db;
        }

        public async Task<Requete> CreateRequestAsync(Requete requete, List<(Stream Stream, string FileName, long Size)>? files = null)
        {
            requete.CreatedAt = DateTime.UtcNow;
            requete.Status = RequestStatus.Submitted;

            await _requeteRepo.AddAsync(requete);

            if (files != null)
            {
                foreach (var file in files)
                    await SaveFileAsync(requete.Id, file.Stream, file.FileName, file.Size);
            }

            return requete;
        }

        public async Task<RequeteFichier> AddFileAsync(int requeteId, Stream fileStream, string fileName, long size)
            => await SaveFileAsync(requeteId, fileStream, fileName, size);

        public async Task<bool> ReplaceFileAsync(int fileId, Stream newStream, string newFileName, long newSize)
        {
            var file = await _db.RequeteFichiers.FindAsync(fileId);
            if (file == null) return false;

            DeletePhysicalFile(file.FilePath);

            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(newFileName)}";
            Directory.CreateDirectory(UploadsDir);
            var filePath = Path.Combine(UploadsDir, safeFileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
                await newStream.CopyToAsync(fs);

            file.FilePath = $"/uploads/requests/{safeFileName}";
            file.FileName = newFileName;
            file.FileSize = newSize;
            file.UploadedAt = DateTime.UtcNow;
            file.ReviewStatus = FileReviewStatus.Pending;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _db.RequeteFichiers.FindAsync(fileId);
            if (file == null) return false;

            DeletePhysicalFile(file.FilePath);
            _db.RequeteFichiers.Remove(file);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReviewFileAsync(int fileId, FileReviewStatus status, string? comment = null)
        {
            var file = await _db.RequeteFichiers.FindAsync(fileId);
            if (file == null) return false;

            file.ReviewStatus = status;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<Requete>> GetRequestsForUserAsync(int userId)
            => await _requeteRepo.GetByDemandeurAsync(userId);

        public async Task<List<Requete>> GetAllRequestsAsync(RequestStatus? filterStatus = null)
        {
            if (filterStatus.HasValue)
                return await _requeteRepo.GetByStatusAsync(filterStatus.Value);
            return await _requeteRepo.GetAllWithDetailsAsync();
        }

        public async Task<Requete?> GetRequestByIdAsync(int id)
            => await _requeteRepo.GetWithDetailsAsync(id);

        public async Task<RequeteCommentaire> AddCommentAsync(int requeteId, int auteurId, string contenu)
        {
            var comment = new RequeteCommentaire
            {
                RequeteId = requeteId,
                AuteurId = auteurId,
                Contenu = contenu,
                Date = DateTime.UtcNow
            };

            _db.RequeteCommentaires.Add(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> UpdateStatusAsync(int requeteId, RequestStatus newStatus, int? reviewerId = null, string? reason = null)
        {
            var requete = await _requeteRepo.GetByIdAsync(requeteId);
            if (requete == null) return false;

            requete.Status = newStatus;
            requete.UpdatedAt = DateTime.UtcNow;

            if (newStatus == RequestStatus.UnderReview || newStatus == RequestStatus.Approved || newStatus == RequestStatus.Rejected)
            {
                if (reviewerId.HasValue)
                    requete.AssigneId = reviewerId;
            }

            if (newStatus == RequestStatus.Rejected && !string.IsNullOrEmpty(reason))
                requete.RejectionReason = reason;

            await _requeteRepo.UpdateAsync(requete);
            return true;
        }

        // --- Private helpers ---

        private async Task<RequeteFichier> SaveFileAsync(int requeteId, Stream stream, string fileName, long size)
        {
            Directory.CreateDirectory(UploadsDir);

            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var filePath = Path.Combine(UploadsDir, safeFileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
                await stream.CopyToAsync(fs);

            var fichier = new RequeteFichier
            {
                RequeteId = requeteId,
                FilePath = $"/uploads/requests/{safeFileName}",
                FileName = fileName,
                FileSize = size,
                UploadedAt = DateTime.UtcNow,
                ReviewStatus = FileReviewStatus.Pending
            };

            _db.RequeteFichiers.Add(fichier);
            await _db.SaveChangesAsync();
            return fichier;
        }

        private static void DeletePhysicalFile(string webPath)
        {
            if (string.IsNullOrEmpty(webPath)) return;
            var physicalPath = Path.Combine("wwwroot", webPath.TrimStart('/'));
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }
    }
}
