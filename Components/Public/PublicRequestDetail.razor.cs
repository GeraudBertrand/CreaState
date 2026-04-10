using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CreaState.Components.Public;

public partial class PublicRequestDetail
{
    [Parameter] public int Id { get; set; }

    [Inject] private RequestService RequestService { get; set; } = default!;
    [Inject] private AuthStateProvider AuthState { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;

    private Requete? Request;
    private string NewCommentText = "";
    private const long MaxFileSize = 1024 * 1024 * 50;

    private bool CanEditFiles => Request?.Status == RequestStatus.Submitted ||
        Request?.Status == RequestStatus.Rejected ||
        Request?.Fichiers.Any(f => f.ReviewStatus == FileReviewStatus.Refused || f.ReviewStatus == FileReviewStatus.NeedsModification) == true;

    protected override async Task OnInitializedAsync()
    {
        var user = AuthState.CurrentUser;
        var request = await RequestService.GetRequestByIdAsync(Id);

        #if DEBUG
        if (request == null) request = LoadMockRequest();
        #endif

        if (request != null && user != null && request.DemandeurId == user.Id)
        {
            Request = request;
        }
    }

    #if DEBUG
    private Requete LoadMockRequest()
    {
        var mockUser = new Membre
        {
            Id = 16, FirstName = "Nathan", LastName = "Simon",
            Email = "nathan.simon@edu.devinci.fr",
            UserRoles = [new AppUserRole { RoleId = 2, Role = new Role { Id = 2, DisplayName = "Membre" } }]
        };
        var mockManager = new Membre
        {
            Id = 12, FirstName = "Hugo", LastName = "Petit",
            Email = "hugo.petit@edu.devinci.fr",
            UserRoles = [new AppUserRole { RoleId = 3, Role = new Role { Id = 3, DisplayName = "Resp. Technique" } }]
        };

        return new Requete
        {
            Id = Id,
            Type = RequestType.FDM,
            Title = "Support de téléphone personnalisé",
            Description = "Impression d'un support ajustable en PLA noir, avec angle réglable et passage pour câble de charge.",
            Status = RequestStatus.Submitted,
            DemandeurId = 16,
            Demandeur = mockUser,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Fichiers = new List<RequeteFichier>
            {
                new() { Id = 1, RequeteId = Id, FileName = "support_base.stl", FilePath = "#", FileSize = 245000, UploadedAt = DateTime.UtcNow.AddDays(-2), ReviewStatus = FileReviewStatus.Accepted },
                new() { Id = 2, RequeteId = Id, FileName = "support_bras.stl", FilePath = "#", FileSize = 180000, UploadedAt = DateTime.UtcNow.AddDays(-2), ReviewStatus = FileReviewStatus.Refused },
                new() { Id = 3, RequeteId = Id, FileName = "clip_cable.3mf", FilePath = "#", FileSize = 52000, UploadedAt = DateTime.UtcNow.AddDays(-1), ReviewStatus = FileReviewStatus.Pending },
            },
            Commentaires = new List<RequeteCommentaire>
            {
                new() { Id = 1, RequeteId = Id, AuteurId = 12, Auteur = mockManager, Contenu = "Le fichier support_bras.stl a une épaisseur trop fine au niveau de la charnière.", Date = DateTime.UtcNow.AddDays(-1).AddHours(-3) },
                new() { Id = 2, RequeteId = Id, AuteurId = 16, Auteur = mockUser, Contenu = "D'accord, je corrige ça ce soir et je renvoie le fichier. Merci pour le retour !", Date = DateTime.UtcNow.AddDays(-1).AddHours(-1) },
            }
        };
    }
    #endif

    internal static string GetStatusCssClass(RequestStatus s) => s switch
    {
        RequestStatus.Submitted => "badge-info",
        RequestStatus.UnderReview => "badge-warning",
        RequestStatus.Approved => "badge-success",
        RequestStatus.Rejected => "badge-danger",
        RequestStatus.InProgress => "badge-primary",
        RequestStatus.Completed => "badge-success",
        RequestStatus.Cancelled => "badge-secondary",
        _ => ""
    };

    internal static string GetFileStatusCss(FileReviewStatus s) => s switch
    {
        FileReviewStatus.Pending => "file-pending",
        FileReviewStatus.Accepted => "file-accepted",
        FileReviewStatus.Refused => "file-refused",
        FileReviewStatus.NeedsModification => "file-modify",
        _ => ""
    };

    internal static string GetFileSizeLabel(long size) => size switch
    {
        < 1024 => $"{size} o",
        < 1024 * 1024 => $"{size / 1024.0:F1} Ko",
        _ => $"{size / 1024.0 / 1024.0:F2} Mo"
    };

    private void GoBack() => NavManager.NavigateTo("/public/my-requests");

    private async Task SendComment()
    {
        if (Request == null || string.IsNullOrWhiteSpace(NewCommentText)) return;
        var user = AuthState.CurrentUser;
        if (user == null) return;

        await RequestService.AddCommentAsync(Request.Id, user.Id, NewCommentText.Trim());
        NewCommentText = "";
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

    private async Task HandleAddFile(InputFileChangeEventArgs e)
    {
        if (Request == null) return;
        var file = e.File;
        if (file.Size > MaxFileSize) return;

        using var stream = file.OpenReadStream(MaxFileSize);
        await RequestService.AddFileAsync(Request.Id, stream, file.Name, file.Size);
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

    private async Task HandleReplaceFile(InputFileChangeEventArgs e, int fileId)
    {
        var file = e.File;
        if (file.Size > MaxFileSize) return;

        using var stream = file.OpenReadStream(MaxFileSize);
        await RequestService.ReplaceFileAsync(fileId, stream, file.Name, file.Size);
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

    private async Task DeleteFile(int fileId)
    {
        await RequestService.DeleteFileAsync(fileId);
        Request = await RequestService.GetRequestByIdAsync(Id);
    }
}
