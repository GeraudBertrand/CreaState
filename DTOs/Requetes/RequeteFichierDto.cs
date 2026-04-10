namespace CreaState.DTOs.Requetes
{
    public class RequeteFichierDto
    {
        public int Id { get; set; }
        public int RequeteId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public string ReviewStatusLabel { get; set; } = string.Empty;
        public string StatusCssClass { get; set; } = string.Empty;
        public string FileSizeLabel { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
