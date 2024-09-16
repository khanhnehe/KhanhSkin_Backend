namespace KhanhSkin_BackEnd.Services
{
    using CloudinaryDotNet.Actions;
    using System.IO;
    using System.Threading.Tasks;

    public interface ICloudinaryService
    {
        Task<ImageUploadResult> UploadImageAsync(Stream imageStream, string publicId);
        Task<DeletionResult> DeleteImageAsync(string publicId);
        string GetImageUrl(string publicId);
    }
}
