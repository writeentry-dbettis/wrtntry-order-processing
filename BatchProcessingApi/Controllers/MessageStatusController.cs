using BatchProcessing.Common.Models;
using BatchProcessing.Common.Models.Requests;
using BatchProcessingApi.Hubs;
using BatchProcessingApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BatchProcessingApi.Controllers
{
    [Route("api/message-status")]
    [ApiController]
    public class MessageStatusController : ControllerBase
    {
        private readonly IHubContext<ChatHub, IChatClient> _hubContext;

        public MessageStatusController(IHubContext<ChatHub, IChatClient> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] MessageStatusUpdateRequest statusUpdate, CancellationToken cancellationToken)
        {
            if (statusUpdate == null)
            {
                return BadRequest("The status request did not contain any data");
            }

            var batchId = statusUpdate.BatchId;

            if (!string.IsNullOrEmpty(batchId))
            {
                await _hubContext.Clients.Groups(batchId).StatusChanged(
                    statusUpdate.ItemId!, 
                    statusUpdate.Status.Name!);
            }

            return Accepted();
        }
    }
}
