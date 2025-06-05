namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IFilesValidator
{
    public bool ValidateFile(MemoryStream memoryStream, string reactionImageFileName);
}