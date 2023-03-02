using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DeepSpeechClient;
using NAudio.Wave;

namespace RaspberryPiWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceRecognition : ControllerBase
    {
        private static DeepSpeechClient.DeepSpeech deepSpeechClient = new DeepSpeechClient.DeepSpeech(@".\deepspeech-0.9.3-models.pbmm");
        private static bool init = false;
        private static object syncroot = new object();
        public VoiceRecognition()
        {
            if (!init)
            {
                lock (syncroot)
                {
                    if (!init)
                    {
                        deepSpeechClient.EnableExternalScorer(@".\deepspeech-0.9.3-models.scorer");
                        init = true;
                    }

                }
            }
        }

        [HttpPost]
        public SpeechRecognitionResponse SpeechRecognition(SpeechRecognitionRequest request)
        {

            var rawBuffer = DownSample(request.Data);
            WaveBuffer buffer = new WaveBuffer(rawBuffer);
            var interpreted = deepSpeechClient.SpeechToText(buffer.ShortBuffer, (uint)buffer.MaxSize / 2);
            return new SpeechRecognitionResponse() { Text = interpreted };

        }
        private static byte[] DownSample(byte[] rawBuffer)
        {
            using (WaveStream? ws = new WaveFileReader(new MemoryStream(rawBuffer)))
            {
                if (ws.WaveFormat.SampleRate > 16000 || ws.WaveFormat.Channels > 1)
                {
                    Console.WriteLine("Too High of Bitrate, converting...");
                    using (WaveFormatConversionStream strm = new WaveFormatConversionStream(new WaveFormat(16000, 1), ws))
                    using (var convertedms = new MemoryStream())
                    {
                        WaveFileWriter.WriteWavFileToStream(convertedms, strm);
                        rawBuffer = convertedms.ToArray();
                    }
                }
                return rawBuffer;
            }
        }




    }

}
