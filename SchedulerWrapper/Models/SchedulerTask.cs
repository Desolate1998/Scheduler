
namespace SchedulerWrapper.Models
{
    public class SchedulerTask<T>
    {

        public TaskType Type { get; set; }
        public TaskLifeCycle LifeCycle { get; set; }
        public string Name { get; set; }
        public string CronExpression { get; }
        public T JobAdditionalDetails { get; set; }
    
    
        public SchedulerTask(string name, string cronExpression, T jobAdditionalDetails, TaskType type = TaskType.Api, TaskLifeCycle lifeCycle=TaskLifeCycle.Permanent)
        {
            Type = type;
            LifeCycle = lifeCycle;
            Name = name;
            CronExpression = cronExpression;
            JobAdditionalDetails = jobAdditionalDetails;
        }

    }
}