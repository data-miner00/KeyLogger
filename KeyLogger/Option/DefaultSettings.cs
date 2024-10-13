namespace KeyLogger.Option;

public sealed class DefaultSettings
{
    public int MaximumKeystrokeDisplayCount { get; set; }

    public int TimerTickInMilliseconds { get; set; }

    public int IdleTimedOutInMilliseconds { get; set; }

    public int StartupDelayInMilliseconds { get; set; }
}
