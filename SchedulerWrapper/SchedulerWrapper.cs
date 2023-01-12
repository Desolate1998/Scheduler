using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using SchedulerWrapper.Models;

namespace SchedulerWrapper
{
  /// <summary>
  /// Class wrapper for IScheduler
  /// </summary>
  /// <typeparam name="TJobType">The type of job being added to the scheduler</typeparam>
  /// <typeparam name="TTaskType">The job type that will be used</typeparam>
  public class SchedulerWrapper<TJobType,TTaskType> where TJobType : IJob
  {
    private readonly ILogger _logger;
    private List<SchedulerTask<TTaskType>> _tasks;
    private  IScheduler _scheduler;

    /// <summary>
    /// Constructor that will startup the scheduler
    /// </summary>
    /// <param name="logger">The logger to log information</param>
    /// <param name="tasks">The tasks that will be executed</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public SchedulerWrapper(ILogger logger, List<SchedulerTask<TTaskType>> tasks)
    {
      _logger = logger ?? throw new NullReferenceException($"{nameof(logger)} is null");
      _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
      _ = StartUp();
     }
    
    /// <summary>
    /// The startup class for the scheduler will start add and start the tasks.
    /// </summary>
    private async Task StartUp()
    {
      ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
      _scheduler = await schedulerFactory.GetScheduler();

      _logger.Log(LogLevel.Information, "Scheduler started");
      await AddJobs();
      _ = _scheduler.Start();
    }

    /// <summary>
    /// Removes a task form the scheduler
    /// </summary>
    /// <param name="name">name of the task</param>
    public void RemoveTask(string name)
    {
      _scheduler.UnscheduleJob(new TriggerKey(name));
      _tasks = _tasks.Where(x => x.Name != name).ToList();
    }

    public void AddTask(SchedulerTask<TTaskType> task)
    {
      _tasks.Add(task);
      Refresh();
    }

    public void Refresh(List<SchedulerTask<TTaskType>> tasks = null)
    {
      _scheduler.Clear();

      //Check if new tasks were sent through, if they were add them and then refresh else just refresh
      if(tasks != null)
      {
        _tasks = tasks;
      }
      _ = AddJobs();
    }

    /// <summary>
    /// Shut down the Scheduler
    /// </summary>
    public void Shutdown()
    {
      _scheduler.Shutdown();
    }

    /// <summary>
    /// Add a new job to the scheduler
    /// </summary>
    private async Task AddJobs()
    {
      foreach (var item in _tasks)
      {
        var job = JobBuilder.Create<TJobType>().WithIdentity(item.Name).UsingJobData("job", JsonConvert.SerializeObject(item)).Build();
        var trigger = TriggerBuilder.Create().WithIdentity(item.Name).WithCronSchedule(item.CronExpression).Build();
        await _scheduler.ScheduleJob(job, trigger);
      }
    }
  }
}
