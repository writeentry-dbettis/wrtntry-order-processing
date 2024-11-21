using BatchProcessingApi.Interfaces;
using BatchProcessingApi.Models;
using BatchProcessing.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BatchProcessing.Common.Models;

namespace BatchProcessingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatchController : ControllerBase
    {
        private readonly IBatchProcessor _batchProcessor;
        private readonly ILogger<BatchController> _logger;

        public BatchController(IBatchProcessor batchProcessor, ILogger<BatchController> logger)
        {
            _batchProcessor = batchProcessor;
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
            var results = new List<QueueResult>();

            foreach (var file in Request.Form.Files) 
            {
                var fileResults = _batchProcessor.ProcessBatchFile<Order>(
                    file.OpenReadStream(), 
                    QueueTopic.OrderProcessing, 
                    newBatchId.ToString(),
                    cancellationToken).WithCancellation(cancellationToken);

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
    }
}
