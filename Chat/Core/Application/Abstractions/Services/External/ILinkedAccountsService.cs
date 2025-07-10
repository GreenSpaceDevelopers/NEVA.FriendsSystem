using Application.Dtos.Responses.Profile;

namespace Application.Abstractions.Services.External;

/// <summary>
/// Сервис для получения привязанных аккаунтов пользователей из основной системы NEVA
/// </summary>
public interface ILinkedAccountsService
{
    /// <summary>
    /// Получить список привязанных аккаунтов пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список привязанных аккаунтов</returns>
    Task<IReadOnlyList<LinkedAccountDto>> GetLinkedAccountsAsync(Guid userId, CancellationToken cancellationToken = default);
} 