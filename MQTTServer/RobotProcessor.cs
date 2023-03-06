using MQTTServer.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTServer.Robot
{
    public class RobotAudioProcessor
    {
        private readonly MessageAggregator _aggregator;
        private readonly TimeSpan _silenceTimeout;

        private DateTimeOffset _lastAudioTime;

        public RobotAudioProcessor(MessageAggregator aggregator, TimeSpan silenceTimeout, Action<Dictionary<Topic, IBaseMessage>> dataHandler)
        {
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            _silenceTimeout = silenceTimeout;
        }

        public void StartProcessing(CancellationToken cancellationToken)
        {
            DateTimeOffset? firstAudioTime = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                var messages = new List<IBaseMessage>();

                // Listen for audio messages from the aggregator and log the time of the last audio message
                while (!cancellationToken.IsCancellationRequested)
                {
                    var currentMessages = _aggregator.GetMessagesAroundTime(_lastAudioTime);
                    if (!currentMessages.ContainsKey(Topic.Audio))
                    {
                        // No new audio messages, wait for a bit before checking again
                        Thread.Sleep(100);
                        continue;
                    }

                    messages.Add(currentMessages[Topic.Audio]);
                    _lastAudioTime = currentMessages[Topic.Audio].Timestamp;

                    if (firstAudioTime == null)
                    {
                        firstAudioTime = _lastAudioTime;
                    }
                }

                // If no audio messages arrive for the timeout period, gather all messages since the first audio message and call the data handler
                var timeoutTime = _lastAudioTime.Add(-_silenceTimeout);
                while (_lastAudioTime > timeoutTime)
                {
                    Thread.Sleep(100);
                    var currentMessages = _aggregator.GetMessagesAroundTime(_lastAudioTime);
                    messages.AddRange(currentMessages.Values);
                    _lastAudioTime = currentMessages.Values.Any() ? currentMessages.Values.Last().Timestamp : _lastAudioTime;
                }

                if (firstAudioTime != null)
                {
                    var collectedData = _aggregator.GetMessagesBetweenTimes(firstAudioTime.Value, _lastAudioTime);
                    HandleData(collectedData);
                    firstAudioTime = null;
                }
            }
        }

        private void HandleData(Dictionary<Topic, IBaseMessage> collectedData)
        {
            // Handle the collected data
            var AudioData = ((AudioMessage) collectedData[Topic.Audio]).Data; //should be a wav file.

        }
    }
}