namespace KhanhSkin_BackEnd.Services
{
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using System.IO;
    using System.Threading.Tasks;

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
                // Sử dụng FileDescription với luồng (Stream)
                File = new FileDescription(publicId, imageStream),
                PublicId = publicId
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult;
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
            return deletionResult;
        }

        public string GetImageUrl(string publicId)
{
    var url = _cloudinary.Api.UrlImgUp
        .BuildUrl(publicId); // Không sử dụng tham số 'transformation'
    return url;
}

    }

}
