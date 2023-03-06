using MQTTServer.Message;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MQTTServer.Tests
{
    [TestClass]
    public class MessageAggregatorTests
    {
        private MessageAggregator _aggregator;
        private List<IBaseMessage> _messages;

        [TestInitialize]
        public void Initialize()
        {
            _aggregator = new MessageAggregator(TimeSpan.FromSeconds(10), TimeSpan.FromHours(1));
            _messages = new List<IBaseMessage>();
        }

        [TestMethod]
        public void GetMessagesAroundTime_ReturnsEmptyDictionary_WhenNoMessages()
        {
            // Arrange
            var time = DateTimeOffset.Now;

            // Act
            var result = _aggregator.GetMessagesAroundTime(time);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetMessagesAroundTime_ReturnsExpectedMessages_WhenMessagesMatchTime()
        {
            // Arrange
            var time = DateTimeOffset.Now;
            var audioMessage = new AudioMessage { Timestamp = time, Data = new byte[] { 1, 2, 3 } };
            var videoMessage = new VideoMessage { Timestamp = time, Data = new byte[] { 4, 5, 6 } };
            _messages.Add(audioMessage);
            _messages.Add(videoMessage);
            _aggregator.ProcessMessages(_messages);

            // Act
            var result = _aggregator.GetMessagesAroundTime(time);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(Topic.Audio));
            Assert.IsTrue(result.ContainsKey(Topic.Video));
            Assert.IsTrue(Enumerable.SequenceEqual(audioMessage.Data, ((AudioMessage)(result[Topic.Audio])).Data));
            Assert.IsTrue(Enumerable.SequenceEqual(videoMessage.Data, ((VideoMessage)(result[Topic.Video])).Data));
        }

        [TestMethod]
        public void GetMessagesAroundTime_ReturnsExpectedMessages_WhenMessagesWithinWindow()
        {
            // Arrange
            var time = DateTimeOffset.Now;
            var audioMessage = new AudioMessage { Timestamp = time.AddSeconds(-5), Data = new byte[] { 1, 2, 3 } };
            var videoMessage = new VideoMessage { Timestamp = time.AddSeconds(-8), Data = new byte[] { 4, 5, 6 } };
            _messages.Add(audioMessage);
            _messages.Add(videoMessage);
            _aggregator.ProcessMessages(_messages);

            // Act
            var result = _aggregator.GetMessagesAroundTime(time);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(Topic.Audio));
            Assert.IsTrue(result.ContainsKey(Topic.Video));
            Assert.IsTrue(Enumerable.SequenceEqual(audioMessage.Data, ((AudioMessage)(result[Topic.Audio])).Data));
            Assert.IsTrue(Enumerable.SequenceEqual(videoMessage.Data, ((VideoMessage)(result[Topic.Video])).Data));
        }

        [TestMethod]
        public void GetMessagesAroundTime_ReturnsExpectedMessages_WhenMessagesOutsideWindow()
        {
            // Arrange
            var time = DateTimeOffset.Now;
            var audioMessage = new AudioMessage { Timestamp = time.AddSeconds(-20), Data = new byte[] { 1, 2, 3 } };
            var videoMessage = new VideoMessage { Timestamp = time.AddSeconds(-15), Data = new byte[] { 4, 5, 6 } };
            _messages.Add(audioMessage);
            _messages.Add(videoMessage);
            _aggregator.ProcessMessages(_messages);

            // Act
            var result = _aggregator.GetMessagesAroundTime(time);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public void GetMessagesAroundTime_ReturnsExpectedMessages_WhenMessagesMatchTimeRange()
        {
            // Arrange
            var time = DateTimeOffset.Now;
            var audioMessageInRange = new AudioMessage { Timestamp = time.AddSeconds(-5), Data = new byte[] { 1, 2, 3 } };
            var videoMessageInRange = new VideoMessage { Timestamp = time.AddSeconds(-8), Data = new byte[] { 4, 5, 6 } };
            var audioMessageOutOfRange = new AudioMessage { Timestamp = time.AddSeconds(-15), Data = new byte[] { 7, 8, 9 } };
            var videoMessageOutOfRange = new VideoMessage { Timestamp = time.AddSeconds(-20), Data = new byte[] { 10, 11, 12 } };
            _messages.Add(audioMessageInRange);
            _messages.Add(videoMessageInRange);
            _messages.Add(audioMessageOutOfRange);
            _messages.Add(videoMessageOutOfRange);
            _aggregator.ProcessMessages(_messages);

            // Act
            var result = _aggregator.GetMessagesAroundTime(time);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(Topic.Audio));
            Assert.IsTrue(result.ContainsKey(Topic.Video));
            Assert.IsTrue(Enumerable.SequenceEqual(audioMessageInRange.Data, ((AudioMessage)(result[Topic.Audio])).Data));
            Assert.IsTrue(Enumerable.SequenceEqual(videoMessageInRange.Data, ((VideoMessage)(result[Topic.Video])).Data));
        }
    }

}
