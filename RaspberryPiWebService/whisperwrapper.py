import whisper
import ffmpeg
class WhisperWrapper:
   #make these static so we can speed up future executions
    _model = whisper.load_model('base', in_memory=True)    
    _options = whisper.DecodingOptions(fp16 = False)
    _probs = None
  
    def __init__(self) -> None:
        pass

     
    def ProcessAudio(self, audiopath):               
        audio = whisper.load_audio(audiopath)
        audio= whisper.pad_or_trim(audio)
        mel = whisper.log_mel_spectrogram(audio).to(WhisperWrapper._model.device)
        if WhisperWrapper._probs == None:
            print('detecting language');
            WhisperWrapper._probs = WhisperWrapper._model.detect_language(mel) #assume going forward all will be the same language.
        result = whisper.decode(WhisperWrapper._model, mel, WhisperWrapper._options)
        return result.text
