using NAudio;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NAudio.Utils;
using System.Diagnostics;

class Program
{
    static async Task Main(string[] args)
    {
        var endpointUrl = "http://localhost:5132/api/VoiceRecognitionWhisper";
        //var endpointUrl = "http://localhost:5000/api/VoiceRecognitionWhisper";



        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            int i = 0;
            
            foreach (var audioSegment in RecordAudio())
            {
               // System.IO.File.WriteAllBytes($"{i++}.wav", audioSegment);
                var request = new SpeechRecognitionRequest { Data = audioSegment };
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpointUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var speechRecognitionResponse = JsonConvert.DeserializeObject<SpeechRecognitionResponse>(responseContent);
                    if (!string.IsNullOrWhiteSpace(speechRecognitionResponse.Text))
                    {
                        Console.WriteLine($"Recognition result: {speechRecognitionResponse.Text}");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to recognize speech: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
    }
    public static int silenceCounter = 0;
    public static IEnumerable<byte[]> RecordAudio()
    {
        var format = new WaveFormat(44100, 16, 1); // 44.1kHz 16-bit mono
        using (var waveIn = new WaveInEvent())
        {
            waveIn.BufferMilliseconds = 100;
            waveIn.NumberOfBuffers = 3;
            waveIn.WaveFormat = format;
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;

            var captureStream = new MemoryStream();
            var waveWriter = new WaveFileWriter(new IgnoreDisposeStream(captureStream), format);

            waveIn.StartRecording();

            bool hasData = false;
            while (_recording || hasData)
            {
                byte[] segment = null;
                while (!_audioData.TryDequeue(out segment))
                {
                    Thread.Sleep(100);
                }


                if (segment != null)
                {
                    var wavData = segment;
                    var ms = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                    writer.Write((int)(wavData.Length + 36)); // File Size - 8
                    writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                    writer.Write(Encoding.ASCII.GetBytes("fmt "));
                    writer.Write((int)16); // chunk size
                    writer.Write((short)1); // format code
                    writer.Write((short)1); // channels
                    writer.Write((int)format.SampleRate); // sample rate
                    writer.Write((int)(format.SampleRate * ((format.BitsPerSample * format.Channels) / 8))); // byte rate
                    writer.Write((short)(format.Channels * (format.BitsPerSample / 8))); // block align
                    writer.Write((short)format.BitsPerSample); // bits per sample
                    writer.Write(Encoding.ASCII.GetBytes("data"));
                    writer.Write((int)wavData.Length+8820);
                    writer.Write(new byte[8820]);
                    writer.Write(wavData);
                    yield return ms.ToArray();
                    captureStream.Seek(0, SeekOrigin.Begin);
                    captureStream.SetLength(0);
                    silenceCounter = 0;
                }
                hasData = false;
            }

            waveIn.StopRecording();
            waveIn.Dispose();
            waveWriter.Dispose();
            captureStream.Dispose();
        }
    }
    private static readonly Queue<byte[]> _audioData = new Queue<byte[]>();
    private static bool _recording = true;
    private static List<byte> _audioBuffer = new List<byte>();
    private static double _silenceThreshold = 0.01;
    private static double _silenceDuration = 0.25;
    private static Stopwatch _silenceStopwatch = new Stopwatch();
    private static void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
    {
        var buffer = new byte[e.BytesRecorded];
        Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

        var rms = CalculateRMSLevel(buffer);
        if (rms < _silenceThreshold)
        {

            // we just encountered silence
            if (!_silenceStopwatch.IsRunning)
            {
                _silenceStopwatch.Start();
                //_audioBuffer.AddRange(new byte[buffer.Length*2]);//prepend some silence.
                _audioBuffer.AddRange(buffer);
            }

        }
        else
        {
            //Console.WriteLine($"RMS Treshold exceeded: {rms}");
            // we just encountered audio
            if (_silenceStopwatch.IsRunning)
            {
                _silenceStopwatch.Stop();
                _silenceStopwatch.Reset();
            }
            _audioBuffer.AddRange(buffer);
        }

        // check if we've been silent for longer than the silence duration
        if (_silenceStopwatch.ElapsedMilliseconds >= _silenceDuration * 1000)
        {
            // check if we have any audio to enqueue
            if (_audioBuffer.Count > 0)
            {
                _audioBuffer.AddRange(buffer);
                _audioData.Enqueue(_audioBuffer.ToArray());
                _audioBuffer.Clear();
            }
        }
    }
    public static double CalculateRMSLevel(byte[] buffer)
    {
        const int BYTES_PER_SAMPLE = 2; // 16-bit audio
        var samples = new float[buffer.Length / BYTES_PER_SAMPLE];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (short)(buffer[i * BYTES_PER_SAMPLE] | (buffer[i * BYTES_PER_SAMPLE + 1] << 8)) / 32768f;
        }
        double rms = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            rms += Math.Pow(samples[i], 2);
        }
        rms /= samples.Length;
        rms = Math.Sqrt(rms);
        return rms;
    }
    private static void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
    {
        _recording = false;
    }
}
public class SpeechRecognitionRequest
{
    [JsonProperty("data")]
    public byte[] Data { get; set; }
}

public class SpeechRecognitionResponse
{
    [JsonProperty("text")]
    public string Text { get; set; }
}
