namespace AbyssIrc.Server.Core.Data.Internal.Services;

public class ProcessQueueConfig
{
    public int MaxParallelTask { get; set; } = Environment.ProcessorCount / 2;
}
