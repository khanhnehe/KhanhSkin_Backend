using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KhanhSkin_BackEnd.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ImageUploadResult> UploadImageAsync(Stream imageStream, string publicId)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(publicId, imageStream),
                PublicId = publicId
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deletionParams);
        }

        public string GetImageUrl(string publicId)
        {
            return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
        }

        public bool ValidateCloudinaryUrl(string url)
        {
            // Kiểm tra xem URL có phải là URL Cloudinary hợp lệ hay không
            var cloudinaryUrlPattern = @"^https://res\.cloudinary\.com/.*$";
            return Regex.IsMatch(url, cloudinaryUrlPattern);
        }

        public string ExtractPublicIdFromUrl(string url)
        {
            // Trích xuất public ID từ URL Cloudinary
            var match = Regex.Match(url, @"/v\d+/(?<publicId>.+)\.[a-zA-Z]+$");
            return match.Success ? match.Groups["publicId"].Value : null;
        }
    }
}