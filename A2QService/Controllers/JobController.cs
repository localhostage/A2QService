using A2QService.Managers;
using A2QService.Model;
using Microsoft.AspNetCore.Mvc;

namespace A2QService.Controllers;

[ApiController]
[Route("job")]
public class JobController : ControllerBase
{
    private readonly ILogger<JobController> _logger;
    
    private JobManager JobManager { get; }

    public JobController(ILogger<JobController> logger, JobManager jobManager)
    {
        _logger = logger;
        JobManager = jobManager;
    }

    [HttpPost(template: "add", Name = "AddJob")]
    [Produces("application/json")]
    public AddJobResponse AddJob(string url, bool isPlaylist = false)
    {
        var id = JobManager.AddJob(url, isPlaylist);
        _logger.Log(LogLevel.Information, "Added job {0} for {1}", id, url);
        
        return new AddJobResponse() { Id = id };
    }
    
    [HttpGet(template: "list", Name = "ListJobs")]
    [Produces("application/json")]
    public List<Job> ListJobs()
    {
        var jobs = JobManager.GetJobs();
        return jobs;
    }
    
    [HttpGet(template: "{jobId}", Name = "GetJob")]
    [Produces("application/json")]
    public ActionResult GetJob(string jobId)
    {
        var job = JobManager.GetJob(jobId);
        
        if (job == null)
        {
            return new NotFoundResult();
        }
        return new JsonResult(job);
    }
}