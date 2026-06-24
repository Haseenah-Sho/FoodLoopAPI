using Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Services
{
    public class FileUploadService(IWebHostEnvironment env) : IFileUploadService
    {
        public async Task<string> UploadAsync(Stream fileStream, string fileName, string folder)
        {
            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileOutStream = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileOutStream);

            return $"/uploads/{folder}/{uniqueFileName}";
        }

        public Task DeleteAsync(string fileUrl)
        {
            var relativePath = fileUrl.TrimStart('/');
            var fullPath = Path.Combine(env.WebRootPath, relativePath.Replace("uploads/", "uploads" + Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }
    }
}