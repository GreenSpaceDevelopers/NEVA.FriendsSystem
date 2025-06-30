using System.Security.Claims;
using System.Text;
using Application.Abstractions.Services.Reporting;
using Infrastructure.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Infrastructure.Services.Reporting;

public class TelegramBugReporter(
    ITelegramBotClient client,
    IHostEnvironment environment,
    IOptions<TelegramConfig> config)
    : IBugReporter
{
    private readonly TelegramConfig _config = config.Value;

    #region IBugReporter

    public async Task ReportAsync(Exception exception, HttpContext request)
    {
        var formattedParts = await FormatAsync(exception, request);

        const int maxParts = 5;
        var partsList = formattedParts.ToList();

        if (partsList.Count > maxParts)
        {
            const string cutMessage = "\n[Остальная часть ошибки урезана, так как сообщение слишком длинное]";

            const int lastIndex = maxParts - 1;
            partsList[lastIndex] += cutMessage;

            partsList = partsList.Take(maxParts).ToList();
        }

        foreach (var part in partsList)
        {
            await client.SendMessage(_config.ErrorsChatId, part);
        }
    }

    #endregion IBugReporter

    private async Task<IEnumerable<string>> FormatAsync(Exception exception, HttpContext context)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Сервис: `{_config.ServiceName}`");
        builder.AppendLine($"Окружение: {environment.EnvironmentName}");
        builder.AppendLine($"Адрес API метода: {context.Request.Path}");
        builder.AppendLine($"Тип запроса: {context.Request.Method}");

        builder.AppendLine($"Хост: {context.Request.Host}");
        builder.AppendLine($"IP клиента: {context.Connection.RemoteIpAddress}");
        if (context.Request.Headers.TryGetValue("User-Agent", out var value))
        {
            builder.AppendLine($"User-Agent: {value}");
        }

        if (context.User?.Identity is { IsAuthenticated: true })
        {
            var claim = context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var userId = claim.Value;
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    builder.AppendLine($"ID пользователя, у которого произошла ошибка: {userId}");
                }
            }
        }

        builder.AppendLine();

        var requestDetails = await FormatRequestAsync(context.Request);

        if (!string.IsNullOrWhiteSpace(requestDetails))
        {
            builder.AppendLine("**Информация о запросе**");
            builder.AppendLine(requestDetails);
            builder.AppendLine();
        }

        builder.AppendLine("**Детали исключения**");
        builder.AppendLine($"Тип: {exception.GetType().FullName}");
        builder.AppendLine($"Сообщение: {exception.Message}");
        builder.AppendLine($"StackTrace: {exception.StackTrace}");

        if (exception.InnerException is not null)
        {
            builder.AppendLine();
            builder.AppendLine("Вложенное исключение:");
            builder.AppendLine($"{exception.InnerException}");
        }

        var buildString = builder.ToString();

        return Split(buildString, 4096);
    }

    private static async Task<string> FormatRequestAsync(HttpRequest request)
    {
        var builder = new StringBuilder();

        if (request.QueryString.HasValue)
        {
            builder.Append($"Строка запроса: {request.QueryString}");
        }

        if (request.Body is { CanRead: true })
        {
            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }

            using var reader = new StreamReader(request.Body, leaveOpen: true);

            var buffer = new char[4096];
            var readCount = await reader.ReadAsync(buffer, 0, buffer.Length);
            var partialBody = new string(buffer, 0, readCount);

            if (!string.IsNullOrEmpty(partialBody))
            {
                var isLikelyBinary = partialBody.Count(c => c < ' ' && c != '\r' && c != '\n' && c != '\t') > 50;
                builder.AppendLine(isLikelyBinary
                    ? "Тело запроса содержит бинарные данные (часть):"
                    : "Тело запроса (первые 4KB):");
                builder.Append(partialBody);

                if (readCount >= buffer.Length)
                {
                    builder.AppendLine();
                    builder.AppendLine("[Тело запроса урезано, т.к. достигнут лимит 4KB]");
                }
            }

            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }
        }

        return builder.ToString();
    }

    private static IEnumerable<string> Split(string text, int partSize)
    {
        var offset = 0;

        while (true)
        {
            if (offset >= text.Length)
            {
                break;
            }

            var length = Math.Min(partSize, text[offset..].Length);
            var part = text.Substring(offset, length);
            offset += length;

            yield return part;
        }
    }
} 