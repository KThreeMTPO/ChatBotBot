using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTServer.Message
{
    public class VideoMessage : BaseMessage<byte[], VideoMessage>, IBaseMessage
    {
        public override Topic Topic => Topic.Video;

        public override VideoMessage CombineData(IEnumerable<VideoMessage> processSegment)
        {
            using (var ms = new MemoryStream())
            {
                var first = processSegment.FirstOrDefault();
                if (first == null)
                    return null;
                foreach (var seg in processSegment)
                    ms.Write(seg.Data);
                return new VideoMessage()
                {
                    Data = ms.ToArray(),
                    Timestamp = first.Timestamp
                };
            }
        }
    }
}
