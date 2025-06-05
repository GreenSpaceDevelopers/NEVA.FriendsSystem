using System.Runtime.CompilerServices;
using Application.Abstractions.Services.Net;
using Infrastructure.Common.Helpers;
using Infrastructure.Configs;
using MessagePack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Services.ApplicationInfrastructure.Network;

public abstract class Queue<T>(IOptions<QueueConfig> options, ILogger<Queue<T>> logger) : IQueue<T> where T : class
{
    private static readonly QueueResponse<T?> noMessageResponse = new(null as T, true);
    private static readonly QueueResponse<T?> invalidMessageResponse = new(null as T, false);
    
    private IChannel? _channel;

    /// <inheritdoc cref="IQueue{T}.WriteAsync"/>
    public async Task WriteAsync(T data, string queueName, string? exchangeName, string? routingKey, bool mandatory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        await InitializeChannelAsync(cancellationToken);
        await _channel!.QueueDeclareAsync( queueName, true, false, false, cancellationToken: cancellationToken);
        
        if (!string.IsNullOrEmpty(exchangeName))
        {
            logger.LogInformation("Creating exchange {exchange}, for {queue}", exchangeName, queueName);
            await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, true, false, cancellationToken: cancellationToken);
        }
        
        var body = MessagePackSerializer.Serialize(data, cancellationToken: cancellationToken);
        var hmac = HMAC.ComputeHMACHash(body, options.Value.HMACKey);
        var signedMessage = new BodyWithHMAC { Data = body, HMAC = hmac };
        var finalBody = MessagePackSerializer.Serialize(signedMessage, cancellationToken: cancellationToken);
        
        await _channel.BasicPublishAsync(exchangeName ?? string.Empty, routingKey ?? string.Empty, mandatory: mandatory, finalBody, cancellationToken);
    }

    /// <inheritdoc cref="IQueue{T}.ReadAsync"/>
    public async Task<QueueResponse<T?>> ReadAsync(string queueName, string exchangeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        await InitializeChannelAsync(cancellationToken);
        var message = await _channel!.BasicGetAsync(queueName, true, cancellationToken);
        
        if (message is null)
        {
            return noMessageResponse;
        }
        
        var messageWithHMAC = MessagePackSerializer.Deserialize<BodyWithHMAC>(message.Body, cancellationToken: cancellationToken);

        if (!HMAC.VerifyHMAC(messageWithHMAC.Data, messageWithHMAC.HMAC, options.Value.HMACKey))
        {
            logger.LogCritical("Received invalid message {queue}, {exchange}, {routingKey}", queueName, message.Exchange, message.RoutingKey);
            
            return invalidMessageResponse;
        }
        
        var data = MessagePackSerializer.Deserialize<T>(messageWithHMAC.Data, cancellationToken: cancellationToken);
        
        return new QueueResponse<T?>(data, true);
    }

    [AsyncMethodBuilder(typeof(AsyncValueTaskMethodBuilder))]
    private async ValueTask InitializeChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is not null && _channel.IsOpen)
        {
            return;
        }
        
        var factory = new ConnectionFactory { HostName = options.Value.Host };
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
    } 
}

file struct BodyWithHMAC
{
    public byte[] Data { get; set; }
    public byte[] HMAC { get; set; }
}