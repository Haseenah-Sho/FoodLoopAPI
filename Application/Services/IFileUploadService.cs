namespace Application.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string folder);
        Task DeleteAsync(string fileUrl);
    }
}