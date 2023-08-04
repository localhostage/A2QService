using A2QService.Managers;
using A2QService.Model;
using Microsoft.AspNetCore.Mvc;

namespace A2QService.Controllers;

[ApiController]
[Route("[controller]")]
public class DownloadController : ControllerBase
{
    private readonly ILogger<DownloadController> _logger;
    
    private JobManager JobManager { get; }

    public DownloadController(ILogger<DownloadController> logger, JobManager jobManager)
    {
        _logger = logger;
        JobManager = jobManager;
    }

    [HttpGet(Name = "DownloadVideo")]
    [Produces("application/json")]
    public AddToQueueResponse Get(string url)
    {
        var id = JobManager.AddJob(url);
        return new AddToQueueResponse() { Id = id };
    }
}