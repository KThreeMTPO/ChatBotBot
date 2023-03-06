using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTServer.Message
{
    public interface IBaseMessage
    {
        public Topic Topic { get; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public abstract class BaseMessage<TData, TMessage> where TMessage : BaseMessage<TData, TMessage>, IBaseMessage
    {
        public abstract Topic Topic { get; }
        public DateTimeOffset Timestamp { get; set; }
        public TData Data { get; set; }

        public abstract TMessage CombineData(IEnumerable<TMessage> processSegment);
    }
}
