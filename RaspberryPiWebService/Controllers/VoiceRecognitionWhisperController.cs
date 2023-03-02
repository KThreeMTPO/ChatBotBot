using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DeepSpeechClient;
using NAudio.Wave;
using Python.Runtime;

namespace RaspberryPiWebService.Controllers
{
    public interface IWhisperPythonWrapper
    {
        public string ProcessAudio(string audio);

    }

    [Route("api/[controller]")]
    [ApiController]
    public class VoiceRecognitionWhisper : ControllerBase
    {
        private static bool init = false;
        private static object syncroot = new object();



        private static PyObject whisperObject;

        public Py.GILState Gilstate { get; }

        private static PyModule scope;

        public VoiceRecognitionWhisper()
        {
            if (!init)
            {
                lock (syncroot)
                {
                    if (!init)
                    {
                        PythonEngine.Initialize();
                        var m_threadState = PythonEngine.BeginAllowThreads();
                        using (Py.GIL())
                        {
                            using (var scope = Py.CreateScope("xx"))
                            {
                                scope.Import("ffmpeg");
                                scope.Import("whisperwrapper");
                                scope.Import("os");
                                object xyz = new object();
                                object fname = new object();
                                scope.Set("FNAME", fname);
                                scope.Set("PROCESSOR", xyz);

                                scope.Exec("PROCESSOR = whisperwrapper.WhisperWrapper()");
                                var result = scope.Eval("os.environ");

                                init = true;
                            }

                        }
                    }
                }
            }
        }
        [HttpPost]
        public SpeechRecognitionResponse SpeechRecognition(SpeechRecognitionRequest request)
        {

            var rawBuffer = DownSample(request.Data);
            WaveBuffer buffer = new WaveBuffer(rawBuffer);
            var fname = Guid.NewGuid().ToString() + ".wav";

            try
            {

                System.IO.File.WriteAllBytes(fname, rawBuffer);
                var fullPath = new System.IO.FileInfo(fname);//.FullName;
                var dir = fullPath.DirectoryName;
                using (Py.GIL())
                {
                    using (var scope = Py.CreateScope("xx"))
                    {
                        scope.Import("ffmpeg");
                        scope.Import("whisperwrapper");
                        scope.Import("os");
                        scope.Set("FNAME", fname);
                        scope.Exec("PROCESSOR = whisperwrapper.WhisperWrapper()");
                        scope.Set("DIR", dir);
                        scope.Set("FNAME", fullPath.FullName);
                        var result = scope.Eval<string>($"PROCESSOR.ProcessAudio(FNAME)");
                        return new SpeechRecognitionResponse() { Text = result };
                    }
                }
            }
            finally
            {
                try
                {
                    System.IO.File.Delete(fname);
                }
                catch { }
            }
            return null;
            //return new SpeechRecognitionResponse() { Text = interpreted };

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
