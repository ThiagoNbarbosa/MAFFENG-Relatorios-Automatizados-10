namespace MAFFENG.Models
{
    public class PreviewModel
    {
        public List<PreviewItem> PreviewItems { get; set; } = new();
        public UploadFormModel FormData { get; set; } = new();
    }

    public class PreviewItem
    {
        public string Type { get; set; } = string.Empty; // "folder" or "image"
        public string Title { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<PreviewImage> Images { get; set; } = new();
    }

    public class PreviewImage
    {
        public string Type { get; set; } = "image";
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class PreviewData
    {
        public List<FolderData> Folders { get; set; } = new();
        public int TotalImages { get; set; }
        public string ProjectName { get; set; } = string.Empty;
    }

    public class FolderData
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<ImageData> Images { get; set; } = new();
    }

    public class ImageData
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
    }
}