using System;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Net.Http;
using System.Threading.Tasks;

namespace VRCTTS
{
    class Voice_Manager
    {
        private Globals globals;

        public List<KeyValuePair<string, List<string>>> languages { get; set; }
        public List<Voice> list_of_voices { get; set; }
        public Voice selected_voice { get; set; }

        private SpeechConfig speechConfig;
        private SpeechSynthesizer speechSynthesizer;

        public Voice_Manager() {
            globals = Globals.Instance;

            languages = new List<KeyValuePair<string, List<string>>>();
            list_of_voices = new List<Voice>();

            // Welsh
            List<string> temp_locale = new List<string> { "cy-GB" };
            languages.Add(new KeyValuePair<string, List<string>>("Welsh", temp_locale));

            // Danish
            temp_locale = new List<string> { "da-dk" };
            languages.Add(new KeyValuePair<string, List<string>>("Danish", temp_locale));

            // German
            temp_locale = new List<string> { "de-AT", "de-CH", "de-DE" };
            languages.Add(new KeyValuePair<string, List<string>>("German", temp_locale));

            // Greek
            temp_locale = new List<string> { "el-GR" };
            languages.Add(new KeyValuePair<string, List<string>>("Greek", temp_locale));

            // English
            temp_locale = new List<string> { "en-AU", "en-CA", "en-GB", "en-HK", "en-IE",
                "en-IN", "en-KE", "en-NG", "en-NZ", "en-PH", "en-SG", "en-TZ", "en-US", "en-ZA" };
            languages.Add(new KeyValuePair<string, List<string>>("English", temp_locale));

            if (globals.azure_key != "" & globals.azure_region != "") 
            {
                speechConfig = SpeechConfig.FromSubscription(globals.azure_key, globals.azure_region);
                speechSynthesizer = new SpeechSynthesizer(speechConfig, null);

                getVoices();
            }
        }

        public void Update() 
        {
            speechConfig = SpeechConfig.FromSubscription(globals.azure_key, globals.azure_region);
            speechSynthesizer = new SpeechSynthesizer(speechConfig, null);

            getVoices();
        }

        /// <summary>
        /// Retrieve voices from selected languages.
        /// </summary>
        public async void getVoices() {
            try
            {
                if (globals.azure_selected_locale.Count != 0 & !globals.azure_key.Equals("") & !globals.azure_region.Equals("")) 
                {
                    list_of_voices.Clear();
                    foreach (string selectedLanguage in globals.azure_selected_locale)
                    {
                        foreach (var language in languages) 
                        {
                            if (selectedLanguage.Equals(language.Key))
                            {
                                foreach (string locale in language.Value) 
                                {
                                    SynthesisVoicesResult response = await speechSynthesizer.GetVoicesAsync(locale);

                                    if (response.Reason == ResultReason.VoicesListRetrieved)
                                    {
                                        foreach (VoiceInfo voice in response.Voices)
                                        {
                                            Voice temp = new Voice(voice.ShortName, voice.StyleList);
                                            list_of_voices.Add(temp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
