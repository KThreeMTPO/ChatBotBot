using MQTTServer.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTServer
{
    public class MessageAggregator
    {
        private readonly TimeSpan _windowSize;
        private readonly TimeSpan _maxMessageAge;
        private readonly ConcurrentDictionary<Topic, SortedList<DateTimeOffset, IBaseMessage>> _topicData;

        public MessageAggregator(TimeSpan windowSize, TimeSpan maxMessageAge)
        {
            _windowSize = windowSize;
            _maxMessageAge = maxMessageAge;
            _topicData = new ConcurrentDictionary<Topic, SortedList<DateTimeOffset, IBaseMessage>>();            
        }

        public void ProcessMessages(IEnumerable<IBaseMessage> data)
        {
            foreach (var datum in data)
            {
                ProcessMessage(datum);
            }
        }
        private void ProcessMessage(IBaseMessage message)
        {
            var topicData = _topicData.GetOrAdd(message.Topic, _ => new SortedList<DateTimeOffset, IBaseMessage>());

            lock (topicData)
            {
                topicData.Add(message.Timestamp, message);
                RemoveOldMessages(topicData);
            }
        }

        private void RemoveOldMessages(SortedList<DateTimeOffset, IBaseMessage> topicData)
        {
            // Remove messages that are older than _maxMessageAge
            var oldestAllowedTimestamp = DateTimeOffset.Now - _maxMessageAge;
            while (topicData.Any() && topicData.Keys[0] < oldestAllowedTimestamp)
            {
                topicData.RemoveAt(0);
            }
        }

        private IBaseMessage CreateMessageForTopic(Topic topic)
        {
            switch (topic)
            {
                case Topic.Audio:
                    return new AudioMessage();
                case Topic.Video:
                    return new VideoMessage();
                default:
                    throw new NotSupportedException($"Unsupported Topic: {topic}");
            }
        }
        public Dictionary<Topic, IBaseMessage> GetMessagesAroundTime(DateTimeOffset time)
        {
            return GetMessagesAroundTime(time, this._windowSize);
        }
        public Dictionary<Topic, IBaseMessage> GetMessagesAroundTime(DateTimeOffset time, TimeSpan ts)
        {
            var result = new Dictionary<Topic, IBaseMessage>();

            foreach (var (topic, topicData) in _topicData)
            {
                lock (topicData)
                {
                    IBaseMessage ret = CreateMessageForTopic(topic);
                    var messagesInRange = topicData.Where(x => x.Key >= time - _windowSize && x.Key <= time + _windowSize)
                        .Select(c => c.Value);
                    if (messagesInRange.Any())
                    {
                        var retType = ret.GetType();
                        var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(retType);
                        var castedMessages = (IEnumerable<object>)castMethod.Invoke(null, new object[] { messagesInRange });
                        ret = (IBaseMessage)retType.GetMethod("CombineData").Invoke(ret, new object[] { castedMessages });
                        result.Add(topic, ret);
                    }
                }
            }

            return result;
        }
     }
}
