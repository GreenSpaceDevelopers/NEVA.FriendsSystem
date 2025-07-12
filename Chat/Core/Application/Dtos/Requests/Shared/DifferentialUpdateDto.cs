using Microsoft.AspNetCore.Http;

namespace Application.Dtos.Requests.Shared;

public class DifferentialUpdateDto
{
    public List<Guid> AttachmentsToDelete { get; set; } = new();
    
    public IFormFileCollection? NewFiles { get; set; }
} 