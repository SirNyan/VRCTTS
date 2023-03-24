using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Xml.Linq;

namespace VRCTTS
{
    class Azure_Speech
    {

        private SpeechConfig speechConfig;
        private AudioConfig audioSTTConfig;
        private AudioConfig audioTTSConfig;
        private SpeechRecognizer speechRecognizer;
        private SpeechSynthesizer speechSynthesizer;

        private Globals globals;

        public string result;
        public bool ttsCompleted;
        public bool ttsFailed;

        public Azure_Speech() {
            globals = Globals.Instance;

            //TODO load and save key + region + voice
            if (globals.azure_key != "" & globals.azure_region != "")
            {
                speechConfig = SpeechConfig.FromSubscription(globals.azure_key, globals.azure_region);
                speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);

                audioSTTConfig = AudioConfig.FromMicrophoneInput(globals.selectedInputDevice.ID);
                audioTTSConfig = AudioConfig.FromSpeakerOutput(globals.selectedOutputDevice.ID);

                speechRecognizer = new SpeechRecognizer(speechConfig, audioSTTConfig);
                speechSynthesizer = new SpeechSynthesizer(speechConfig, audioTTSConfig);

                speechSynthesizer.GetVoicesAsync("en-US");
            }

            result = "";
        }

        /// <summary>
        /// Update configuration variables.
        /// </summary>
        public void Update()
        {
            speechConfig = SpeechConfig.FromSubscription(globals.azure_key, globals.azure_region);

            audioSTTConfig = AudioConfig.FromMicrophoneInput(globals.selectedInputDevice.ID);
            audioTTSConfig = AudioConfig.FromSpeakerOutput(globals.selectedOutputDevice.ID);

            speechRecognizer = new SpeechRecognizer(speechConfig, audioSTTConfig);
            speechSynthesizer = new SpeechSynthesizer(speechConfig, audioTTSConfig);
        }

        /// <summary>
        /// Speech to text.
        /// </summary>
        async public void stt() {
            try
            {
                ttsCompleted = false;
                ttsFailed = false;
                result = "";

                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync().ConfigureAwait(false);
                switch (speechRecognitionResult.Reason)
                {
                    case ResultReason.NoMatch:
                        ttsFailed = true;
                        break;
                    case ResultReason.Canceled:
                        ttsFailed = true;
                        break;
                    case ResultReason.RecognizingSpeech:
                        break;
                    case ResultReason.RecognizedSpeech:
                        result = speechRecognitionResult.Text;
                        ttsCompleted = true;
                        break;
                    #region Rest of the results
                    case ResultReason.RecognizingIntent:
                        break;
                    case ResultReason.RecognizedIntent:
                        break;
                    case ResultReason.TranslatingSpeech:
                        break;
                    case ResultReason.TranslatedSpeech:
                        break;
                    case ResultReason.SynthesizingAudio:
                        break;
                    case ResultReason.SynthesizingAudioCompleted:
                        break;
                    case ResultReason.RecognizingKeyword:
                        break;
                    case ResultReason.RecognizedKeyword:
                        break;
                    case ResultReason.SynthesizingAudioStarted:
                        break;
                    case ResultReason.TranslatingParticipantSpeech:
                        break;
                    case ResultReason.TranslatedParticipantSpeech:
                        break;
                    case ResultReason.TranslatedInstantMessage:
                        break;
                    case ResultReason.TranslatedParticipantInstantMessage:
                        break;
                    case ResultReason.EnrollingVoiceProfile:
                        break;
                    case ResultReason.EnrolledVoiceProfile:
                        break;
                    case ResultReason.RecognizedSpeakers:
                        break;
                    case ResultReason.RecognizedSpeaker:
                        break;
                    case ResultReason.ResetVoiceProfile:
                        break;
                    case ResultReason.DeletedVoiceProfile:
                        break;
                    case ResultReason.VoicesListRetrieved:
                        break;
                    #endregion
                    default:
                        ttsFailed = true;
                        break;
                }
            }
            catch (Exception e)
            {
                //result = e.Message;
                ttsCompleted = true;
            }
        }

        /// <summary>
        /// Text to speech.
        /// </summary>
        async public void tts() {
            try
            {
                string xml = "<speak xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" version=\"1.0\" xml:lang=\"en-US\">" +
                    "<voice name=\"" + globals.azure_selected_voice + "\">";
                if (!globals.azure_selected_style.Equals("")) 
                {
                    xml += "<mstts:express-as style=\"" + globals.azure_selected_style + "\">";
                }
                if (!globals.azure_selected_pitch.Equals("")) 
                {
                    xml += "<prosody pitch=\"" + globals.azure_selected_pitch + "\">";
                }
                xml += result;
                if (!globals.azure_selected_pitch.Equals(""))
                {
                    xml += "</prosody>";
                }
                if (!globals.azure_selected_style.Equals(""))
                {
                    xml += "</mstts:express-as>";
                }
                xml += "</voice></speak>";

                SpeechSynthesisResult speechSynthesizerResult = await speechSynthesizer.SpeakSsmlAsync(xml);
            }
            catch (Exception)
            {
                throw;
                ttsCompleted = true;
            }
        }
    }


}
