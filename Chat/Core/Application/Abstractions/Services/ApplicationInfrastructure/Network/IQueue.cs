namespace Application.Abstractions.Services.Net;

public interface IQueue<T>
{
    /// <summary>
    /// Send data to the queue
    /// if queue not exists, create queue
    /// if exchange Name is not null, create exchange
    /// if routing key is not null, send to routing key
    /// </summary>
    /// <exception cref="ArgumentNullException">if queue Name is null or data is null</exception>
    /// <param Name="data">Data to send</param>
    /// <param Name="queueName">queue Name</param>
    /// <param Name="exchangeName">exchange Name</param>
    /// <param Name="mandatory">mandatory</param>
    /// <param Name="routingKey">routing key</param>
    /// <param Name="cancellationToken">cancellation token</param>
    public Task WriteAsync(T data, string queueName, string? exchangeName, string? routingKey, bool mandatory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Just read from the queue
    /// Return null data when queue is empty or message is invalid
    /// If message is invalid, log it and return is success false
    /// </summary>
    /// <exception cref="ArgumentNullException">if queue Name</exception>
    /// <param Name="queueName">Name of the queue</param>
    /// <param Name="exchangeName">Name of the exchange</param>
    /// <param Name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public Task<QueueResponse<T?>> ReadAsync(string queueName, string exchangeName, CancellationToken cancellationToken = default);
}

public record QueueResponse<T>(T? data, bool isSuccess);