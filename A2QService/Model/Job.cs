namespace A2QService.Model;

public class Job
{
    public string Id { get; set; } = string.Empty;
    
    public string Url { get; set; } = string.Empty;
    
    public DateTime Created { get; set; }
    public DateTime Started { get; set; }
    public DateTime Finished { get; set; }
    
    public double Progress { get; set; }

    public StatusEnum Status { get; set; } = StatusEnum.Waiting;
    
    public bool IsPlaylist { get; set; }

    public enum StatusEnum
    {
        Waiting,
        Running,
        Finished,
        Error
    }
}