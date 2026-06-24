namespace Application.Common.Helpers
{
    public static class FileValidationHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

        public static (bool IsValid, string? ErrorMessage) Validate(string fileName, long fileSizeInBytes)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return (false, $"File type {extension} is not allowed. Allowed types: jpg, png, webp.");

            if (fileSizeInBytes > MaxFileSizeInBytes)
                return (false, "File size exceeds the 5MB limit.");

            return (true, null);
        }
    }
}