using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTServer.Message
{
    public class VideoMessage : BaseMessage<VideoData, VideoMessage>, IBaseMessage
    {
        public override Topic Topic => Topic.Video;

        public override VideoMessage CombineData(IEnumerable<VideoMessage> processSegment)
        {
            
            var first = processSegment.FirstOrDefault();
            if (first == null)
                return null;
            var data = processSegment.SelectMany(c => c.Data.FrameData);
            VideoMessage videoMessage = new VideoMessage()
            {
                Timestamp = first.Timestamp,
                Data = new VideoData(data)
            };
            return videoMessage;
        }
    }
    public class VideoData
    {
        public VideoData(IEnumerable<byte[]> FrameData)
        {
            this.FrameData = FrameData;
        }
        public VideoData(byte[] FrameData)
        {
            this.FrameData = new List<byte[]>() { FrameData };
        }
        public IEnumerable<byte[]> FrameData { get; set; }
    }
}
