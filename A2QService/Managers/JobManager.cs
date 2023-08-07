using System.Diagnostics;
using System.Text.RegularExpressions;
using A2QService.Model;

namespace A2QService.Managers;

public class JobManager
{
    private readonly Regex _progressRegex = new Regex(@"(\d*\.?\d+)?% of");
    private ConfigManager ConfigManager { get; set; }
    private Queue<Job> JobQueue { get; } = new Queue<Job>();
    
    public int TotalJobs => JobQueue.Count;
    
    public JobManager(ConfigManager configManager)
    {
        this.ConfigManager = configManager;
    }

    public string AddJob(string url)
    {
        Console.WriteLine("Adding job for {0}", url);

        // generate a Task
        var job = new Job
        {
            Id = Guid.NewGuid().ToString(),
            Url = url,
            Created = DateTime.Now,
            Started = DateTime.MinValue,
            Finished = DateTime.MinValue,
            Progress = 0,
            Status = Job.StatusEnum.Waiting
        };

        // add the task to the queue
        JobQueue.Enqueue(job);

        // start the task
        TryRunJob();

        return job.Id;
    }

    private void TryRunJob()
    {
        // if there are no jobs in the queue, return
        if (JobQueue.Count == 0)
        {
            return;
        }

        // get the first job in the queue
        var job = JobQueue.Peek();

        // if the job is already running, return
        if (job.Status == Job.StatusEnum.Running)
        {
            return;
        }

        // run the job
        RunJob(job);
    }

    private void RunJob(Job job)
    {
        // set states
        job.Started = DateTime.Now;
        job.Status = Job.StatusEnum.Running;

        // run task in new thread
        var task = Task.Run(() =>
        {
            var args =
                $"-f bv+ba -P \"{ConfigManager.Config.DownloadPath}\" -P \"temp:tmp\" -P \"subtitle:subs\" --embed-subs --write-auto-sub --sub-lang \"en.*\" {job.Url}";
                
            // Create new process start info
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ConfigManager.Config.YtDlPath,
                Arguments = args, 
                UseShellExecute = false, 
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true 
            };
            
            startInfo.EnvironmentVariables["PATH"] += @$";{ConfigManager.Config.FfmpegPath}";

            // Create a new process 
            Process process = new Process
            {
                StartInfo = startInfo // set the start info
            };

            // Register event handler for output
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var output = e.Data;
                    Console.WriteLine($"[{job.Id}] {output}");
                }
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var output = e.Data;
                    Console.WriteLine($"[{job.Id}] {output}");
                    
                    // check if the output contains a progress update
                    var match = _progressRegex.Match(output);
                    if (match.Success)
                    {
                        // get the progress
                        var progressStr = match.Groups[1].Value;
                        if (double.TryParse(progressStr, out var progress))
                        {
                            // update the progress
                            job.Progress = progress;
                            Console.WriteLine("Progress: {0}", progress);
                        }
                    }
                }
            };

            // Start the process
            process.Start();

            // Begin reading the output
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to finish
            process.WaitForExit();

            // Close the process
            process.Close();

            // mark the job as finished
            job.Finished = DateTime.Now;
            job.Status = Job.StatusEnum.Finished;
        });
        
        // log message after job finished
        task.ContinueWith(t =>
        {
            Console.WriteLine($"[{job.Id}] Task finished");
            
            // remove the job from the queue
            JobQueue.Dequeue();

            // try to run the next job
            TryRunJob();
        });
    }
}