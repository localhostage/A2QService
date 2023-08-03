using Microsoft.AspNetCore.Mvc;
using WebApplication1.Managers;
using WebApplication1.Model;

namespace WebApplication1.Controllers;

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