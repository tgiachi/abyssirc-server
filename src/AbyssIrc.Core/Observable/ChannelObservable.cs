using System.Threading.Channels;

namespace AbyssIrc.Core.Observable;

public class ChannelObservable<T> : IObservable<T>
{
    private readonly ChannelReader<T> _reader;

    public ChannelObservable(ChannelReader<T> reader) => _reader = reader;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        var cts = new CancellationTokenSource();

        _ = Task.Run(
            async () =>
            {
                try
                {
                    await foreach (var item in _reader.ReadAllAsync(cts.Token))
                    {
                        observer.OnNext(item);
                    }

                    observer.OnCompleted();
                }
                catch (OperationCanceledException)
                {
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            },
            cts.Token
        );

        return new CancellationDisposable(cts);
    }
}
