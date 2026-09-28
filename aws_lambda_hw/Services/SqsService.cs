using Amazon.SQS;
using Amazon.SQS.Model;
using aws_lambda_hw.Models;
using System.Text.Json;

namespace aws_lambda_hw.Services;

public class SqsService
{
    private readonly IAmazonSQS _sqs;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqsService> _logger;

    public SqsService(
        IAmazonSQS sqs,
        IConfiguration configuration,
        ILogger<SqsService> logger)
    {
        _sqs = sqs;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task SendOrderAsync(
        OrderRequest order,
        CancellationToken cancellationToken)
    {
        // config
        var queueUrl = _configuration["AWS:SqsQueueUrl"];

        if (string.IsNullOrWhiteSpace(queueUrl))
            throw new InvalidOperationException("SQS queue URL is not configured.");

        // serialize
        var json = JsonSerializer.Serialize(order);

        // create request
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = json
        };

        // get response
        var response = await _sqs.SendMessageAsync(
            request, cancellationToken);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            throw new InvalidOperationException("Failed to send order to SQS.");

        _logger.LogInformation($"Order message sent to SQS. MessageId: {response.MessageId}");
    }
}