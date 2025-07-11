using Application.Abstractions.Services.ApplicationInfrastructure.Data;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class FilesValidator : IFilesValidator
{
    private const long MaxFileSizeBytes = 3 * 1024 * 1024; // 3MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

    public bool ValidateFile(MemoryStream memoryStream, string fileName)
    {
        if (memoryStream.Length > MaxFileSizeBytes)
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var isValid = AllowedExtensions.Contains(extension);
        
        if (memoryStream.CanSeek)
        {
            memoryStream.Position = 0;
        }
        
        return isValid;
    }
}