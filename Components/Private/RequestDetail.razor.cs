using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CreaState.Components.Private;

public partial class RequestDetail
{
    [Parameter] public int Id { get; set; }

    [Inject] private PageHeaderService HeaderService { get; set; } = default!;
    [Inject] private RequestService RequestService { get; set; } = default!;
    [Inject] private AuthStateProvider AuthState { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private PrinterService PrinterService { get; set; } = default!;
    [Inject] private EmailService EmailService { get; set; } = default!;

    private Requete? Request;
    private bool IsManager;
    private bool IsOwner;
    private List<Printer> Printers = new();

    private bool IsSendingNotification = false;
    private string? NotificationMessage;

    private bool ShowCommentModal = false;
    private int CommentFileId;
    private string CommentText = "";
    private string NewCommentText = "";

    private const long MaxFileSize = 1024 * 1024 * 50;

    private bool CanEditFiles => IsOwner && (
        Request?.Status == RequestStatus.Submitted ||
        Request?.Status == RequestStatus.Rejected ||
        Request?.Fichiers.Any(f => f.ReviewStatus == FileReviewStatus.Refused || f.ReviewStatus == FileReviewStatus.NeedsModification) == true
    );

    protected override async Task OnInitializedAsync()
    {
        HeaderService.SetTitle($"Demande #{Id}", "Détail de la demande");

        Request = await RequestService.GetRequestByIdAsync(Id);

        #if DEBUG
        if (Request == null) LoadMockRequest();
        #endif

        IsManager = AuthState.HasPermission("manage_requests");

        var currentUser = AuthState.CurrentUser;
        IsOwner = currentUser != null && Request?.DemandeurId == currentUser.Id;

        Printers = PrinterService.GetPrinters();
    }

    #if DEBUG
    private void LoadMockRequest()
    {
        var mockUser = new Membre
        {
            Id = 16, FirstName = "Nathan", LastName = "Simon",
            Email = "nathan.simon@edu.devinci.fr",
            UserRoles = [new AppUserRole { RoleId = 2, Role = new Role { Id = 2, DisplayName = "Membre" } }]
        };

        Request = new Requete
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
                new() { Id = 1, RequeteId = Id, AuteurId = 12, Auteur = new Membre { Id = 12, FirstName = "Hugo", LastName = "Petit", Email = "hugo.petit@edu.devinci.fr", UserRoles = [new AppUserRole { RoleId = 3, Role = new Role { Id = 3, DisplayName = "Resp. Technique" } }] }, Contenu = "Le fichier support_bras.stl a une épaisseur trop fine au niveau de la charnière.", Date = DateTime.UtcNow.AddDays(-1).AddHours(-3) },
                new() { Id = 2, RequeteId = Id, AuteurId = 16, Auteur = mockUser, Contenu = "D'accord, je corrige ça ce soir. Merci pour le retour !", Date = DateTime.UtcNow.AddDays(-1).AddHours(-1) },
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

    private void GoBack() => NavManager.NavigateTo("/private/requests");

    private async Task ReviewFile(int fileId, FileReviewStatus status)
    {
        await RequestService.ReviewFileAsync(fileId, status);
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

    private void OpenCommentModal(int fileId)
    {
        CommentFileId = fileId;
        CommentText = "";
        ShowCommentModal = true;
    }

    private async Task SubmitComment()
    {
        await RequestService.ReviewFileAsync(CommentFileId, FileReviewStatus.NeedsModification, CommentText);
        ShowCommentModal = false;
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

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

    private async Task SendNotification()
    {
        if (Request == null) return;
        IsSendingNotification = true;

        try
        {
            await EmailService.SendRequestReviewNotificationAsync(Request);
            NotificationMessage = "Email envoyé avec succès !";
        }
        catch
        {
            NotificationMessage = "Erreur lors de l'envoi (vérifiez la config SMTP).";
        }
        finally
        {
            IsSendingNotification = false;
        }
    }

    private async Task Approve()
    {
        var membre = AuthState.CurrentMembre;
        await RequestService.UpdateStatusAsync(Id, RequestStatus.Approved, membre?.Id);
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

    private async Task Reject()
    {
        var membre = AuthState.CurrentMembre;
        await RequestService.UpdateStatusAsync(Id, RequestStatus.Rejected, membre?.Id, "Demande rejetée par le responsable.");
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

    private async Task Start()
    {
        await RequestService.UpdateStatusAsync(Id, RequestStatus.InProgress);
        Request = await RequestService.GetRequestByIdAsync(Id);
    }

    private async Task Complete()
    {
        await RequestService.UpdateStatusAsync(Id, RequestStatus.Completed);
        Request = await RequestService.GetRequestByIdAsync(Id);
    }
}
