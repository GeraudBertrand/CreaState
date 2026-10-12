using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

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

        if (request != null && user != null && request.DemandeurId == user.Id)
        {
            Request = request;
        }
    }

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

    private async Task HandleComposerKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendComment();
        }
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
