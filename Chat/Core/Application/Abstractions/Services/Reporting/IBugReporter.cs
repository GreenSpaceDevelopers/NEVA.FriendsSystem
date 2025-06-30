using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Services.Reporting;

public interface IBugReporter
{
    Task ReportAsync(Exception exception, HttpContext request);
} 