using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAuthApp.Interfaces
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IBrowserFile file, string folderName = "blog-images");
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderName = "blog-images");
        bool ValidateImageFile(IBrowserFile file);
    }
}
