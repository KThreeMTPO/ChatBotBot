using System;

public class RobotProcessor
{
    private readonly MessageAggregator _aggregator;
    private readonly TimeSpan _silenceTimeout;
    private readonly Action<Dictionary<Topic, IBaseMessage>> _dataHandler;

    private DateTimeOffset _lastMessageTime;

    public RobotProcessor(MessageAggregator aggregator, TimeSpan silenceTimeout, Action<Dictionary<Topic, IBaseMessage>> dataHandler)
    {
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        _silenceTimeout = silenceTimeout;
        _dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
    }

    public void StartProcessing(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messages = new List<IBaseMessage>();
            var lastMessageTime = _lastMessageTime;

            // Listen for messages from the aggregator and log the time of the last message
            while (!cancellationToken.IsCancellationRequested)
            {
                var currentMessages = _aggregator.GetNewMessagesAfterTime(lastMessageTime);
                if (currentMessages.Count == 0)
                {
                    // No new messages, wait for a bit before checking again
                    Thread.Sleep(100);
                    continue;
                }

                messages.AddRange(currentMessages.Values);
                lastMessageTime = currentMessages[currentMessages.Count - 1].Timestamp;
                _lastMessageTime = lastMessageTime;
            }

            // If no messages arrive for the timeout period, gather the relevant messages and call the data handler
            var timeoutTime = _lastMessageTime.Add(-_silenceTimeout);
            while (_lastMessageTime > timeoutTime)
            {
                Thread.Sleep(100);
            }

            var collectedData = _aggregator.GetMessagesAroundTime(timeoutTime);
            _dataHandler(collectedData);
        }
    }
}
