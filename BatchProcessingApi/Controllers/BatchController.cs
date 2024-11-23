using BatchProcessingApi.Interfaces;
using BatchProcessingApi.Models;
using Microsoft.AspNetCore.Mvc;
using BatchProcessing.Common.Models;
using System.Text.Json;

namespace BatchProcessingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatchController : ControllerBase
    {
        private readonly IBatchProcessor _batchProcessor;
        private readonly IPublishService _publishService;
        private readonly ILogger<BatchController> _logger;

        public BatchController(IBatchProcessor batchProcessor, IPublishService publishService, ILogger<BatchController> logger)
        {
            _batchProcessor = batchProcessor;
            _publishService = publishService;
            _logger = logger;
        }

        [HttpPost("new")]
        public async Task<IActionResult> UploadBatch(CancellationToken cancellationToken) 
        {
            if (Request.Form.Files.Count == 0) 
            {
                return BadRequest("No batch file was found in the API request.");
            }

            var newBatchId = Guid.NewGuid();
            var results = new List<Order>();

            foreach (var file in Request.Form.Files) 
            {
                var fileResults = _batchProcessor.ProcessBatchFile<Order>(
                    file.OpenReadStream(), 
                    cancellationToken);

                await foreach (var result in fileResults)
                {
                    results.Add(result);
                }
            }

            return Ok(new {
                BatchId = newBatchId,
                Results = results
            });
        }

        [HttpPost("{batchId}/queue")]
        public async Task<IActionResult> QueueOrdersToProcess([FromRoute] string batchId, CancellationToken cancellationToken)
        {
            var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(
                Request.BodyReader.AsStream(),
                new JsonSerializerOptions() 
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            if (!(orders?.Any() ?? false))
            {
                return NoContent();
            }

            var count = await _publishService.PublishMessageToTopic(
                QueueTopic.OrderProcessing.TopicId, 
                batchId, 
                cancellationToken, 
                orders.ToArray());

            return Accepted(count);
        }
    }
}
