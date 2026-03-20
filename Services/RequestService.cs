using CreaState.Data;
using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class RequestService
    {
        private readonly AppDbContext _db;
        private const string UploadsDir = "wwwroot/uploads/requests";

        public RequestService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Crée une requête avec plusieurs fichiers.
        /// </summary>
        public async Task<Request> CreateRequestAsync(Request request, List<(Stream Stream, string FileName, long Size)>? files = null)
        {
            request.CreatedAt = DateTime.UtcNow;
            request.Status = RequestStatus.Submitted;

            _db.Requests.Add(request);
            await _db.SaveChangesAsync();

            if (files != null)
            {
                foreach (var file in files)
                {
                    await SaveFileAsync(request.Id, file.Stream, file.FileName, file.Size);
                }
            }

            return request;
        }

        /// <summary>
        /// Ajoute un fichier à une requête existante.
        /// </summary>
        public async Task<RequestFile> AddFileAsync(int requestId, Stream fileStream, string fileName, long size)
        {
            return await SaveFileAsync(requestId, fileStream, fileName, size);
        }

        /// <summary>
        /// Remplace un fichier existant par un nouveau.
        /// </summary>
        public async Task<bool> ReplaceFileAsync(int fileId, Stream newStream, string newFileName, long newSize)
        {
            var file = await _db.RequestFiles.FindAsync(fileId);
            if (file == null) return false;

            // Supprimer l'ancien fichier physique
            DeletePhysicalFile(file.FilePath);

            // Sauvegarder le nouveau
            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(newFileName)}";
            Directory.CreateDirectory(UploadsDir);
            var filePath = Path.Combine(UploadsDir, safeFileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await newStream.CopyToAsync(fs);
            }

            file.FilePath = $"/uploads/requests/{safeFileName}";
            file.OriginalFileName = newFileName;
            file.FileSize = newSize;
            file.UploadedAt = DateTime.UtcNow;
            file.Status = FileReviewStatus.Pending;
            file.ManagerComment = null;

            // Reset notification flag sur la requête
            var request = await _db.Requests.FindAsync(file.RequestId);
            if (request != null) request.NotificationSent = false;

            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Supprime un fichier (disque + BDD).
        /// </summary>
        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _db.RequestFiles.FindAsync(fileId);
            if (file == null) return false;

            DeletePhysicalFile(file.FilePath);
            _db.RequestFiles.Remove(file);
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Review un fichier (accepter/refuser/demander modification).
        /// </summary>
        public async Task<bool> ReviewFileAsync(int fileId, FileReviewStatus status, string? comment = null)
        {
            var file = await _db.RequestFiles.FindAsync(fileId);
            if (file == null) return false;

            file.Status = status;
            file.ManagerComment = comment;

            // Reset notification flag
            var request = await _db.Requests.FindAsync(file.RequestId);
            if (request != null) request.NotificationSent = false;

            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Marque la notification comme envoyée.
        /// </summary>
        public async Task MarkNotificationSentAsync(int requestId)
        {
            var request = await _db.Requests.FindAsync(requestId);
            if (request != null)
            {
                request.NotificationSent = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Request>> GetRequestsForUserAsync(int memberId)
        {
            return await _db.Requests
                .Include(r => r.RequestedBy)
                .Include(r => r.AssignedTo)
                .Include(r => r.Printer)
                .Include(r => r.Files)
                .Where(r => r.RequestedByMemberId == memberId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Request>> GetAllRequestsAsync(RequestStatus? filterStatus = null)
        {
            var query = _db.Requests
                .Include(r => r.RequestedBy)
                .Include(r => r.AssignedTo)
                .Include(r => r.Printer)
                .Include(r => r.Files)
                .AsQueryable();

            if (filterStatus.HasValue)
                query = query.Where(r => r.Status == filterStatus.Value);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<Request?> GetRequestByIdAsync(int id)
        {
            return await _db.Requests
                .Include(r => r.RequestedBy)
                .Include(r => r.AssignedTo)
                .Include(r => r.Printer)
                .Include(r => r.Files)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> UpdateStatusAsync(int requestId, RequestStatus newStatus, int? reviewerId = null, string? reason = null)
        {
            var request = await _db.Requests.FindAsync(requestId);
            if (request == null) return false;

            request.Status = newStatus;

            if (newStatus == RequestStatus.UnderReview || newStatus == RequestStatus.Approved || newStatus == RequestStatus.Rejected)
            {
                request.ReviewedAt = DateTime.UtcNow;
                if (reviewerId.HasValue)
                    request.AssignedToMemberId = reviewerId;
            }

            if (newStatus == RequestStatus.Rejected && !string.IsNullOrEmpty(reason))
                request.RejectionReason = reason;

            if (newStatus == RequestStatus.Completed)
                request.CompletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignMachineAsync(int requestId, string printerId)
        {
            var request = await _db.Requests.FindAsync(requestId);
            if (request == null) return false;

            request.PrinterId = printerId;
            await _db.SaveChangesAsync();
            return true;
        }

        // --- Private helpers ---

        private async Task<RequestFile> SaveFileAsync(int requestId, Stream stream, string fileName, long size)
        {
            Directory.CreateDirectory(UploadsDir);

            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var filePath = Path.Combine(UploadsDir, safeFileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fs);
            }

            var requestFile = new RequestFile
            {
                RequestId = requestId,
                FilePath = $"/uploads/requests/{safeFileName}",
                OriginalFileName = fileName,
                FileSize = size,
                UploadedAt = DateTime.UtcNow,
                Status = FileReviewStatus.Pending
            };

            _db.RequestFiles.Add(requestFile);
            await _db.SaveChangesAsync();
            return requestFile;
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
