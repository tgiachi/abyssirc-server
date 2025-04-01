namespace AbyssIrc.Core.Data.Configs.Sections;

public class ProcessQueueConfig
{
    public int MaxParallelTasks { get; set; } = Environment.ProcessorCount / 2;
}
