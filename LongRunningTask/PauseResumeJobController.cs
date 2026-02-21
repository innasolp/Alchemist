using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace LongRunningTask;

[ApiController]
[Route("api/jobs")]
public class PauseResumeJobController(IBackgroundJobClient backgroundJobClient) : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    [HttpPost("{jobId}/resume")]
    public IActionResult ResumeJob(string jobId)
    {
        var stateChanged = _backgroundJobClient.ChangeState(jobId, new PausedState());

        if (stateChanged)
            return Ok(new { Message = "Task was return to queue." });

        return NotFound();
    }

    [HttpPost("{jobId}/pause")]
    public IActionResult PauseJob(string jobId)
    {
        var stateChanged = _backgroundJobClient.ChangeState(jobId, new PausedState());

        if (stateChanged)
            return Ok(new { Message = "Task was paused." });

        return NotFound();
    }
}
