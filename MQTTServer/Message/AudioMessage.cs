using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTServer.Message
{
    public class AudioMessage : BaseMessage<byte[], AudioMessage>, IBaseMessage
    {
        public override Topic Topic => Topic.Audio;
        public override AudioMessage CombineData(IEnumerable<AudioMessage> processSegment)
        {
            using (var ms = new MemoryStream())
            {
                var first = processSegment.FirstOrDefault();
                if (first == null)
                    return null;
                foreach (var seg in processSegment)
                    ms.Write(seg.Data);
                return new AudioMessage()
                {
                    Data = ms.ToArray(),
                    Timestamp = first.Timestamp
                };
            }
        }
    }
}

