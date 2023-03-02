namespace RaspberryPiWebService
{
    public class SpeechRecognitionRequest
    {
        public byte[] Data { get; set; }
    }

    public class SpeechRecognitionResponse
    {
        public string Text { get; set; }
    }
}
