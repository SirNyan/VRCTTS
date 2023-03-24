using NAudio.CoreAudioApi;
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace VRCTTS
{
    class Globals
    {
        // Audio devices variables.
        public MMDevice selectedInputDevice { get; set; }
        public MMDevice selectedOutputDevice { get; set; }

        // OSC variables.
        public string osc_IPAddress { get; set; }
        public string osc_parameter { get; set; }
        public int osc_port_reciever { get; set; }
        public int osc_port_sender { get; set; }

        // Azure variables.
        public string azure_key { get; set; }
        public string azure_region { get; set; }
        public string azure_selected_voice { get; set; }
        public List<string> azure_selected_locale { get; set; }
        public string azure_selected_pitch { get; set; }
        public string azure_selected_style { get; set; }


        private static readonly object padlock = new object();
        private static Globals instance;
        public static Globals Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        return instance = new Globals();
                    }
                    return instance;
                }
            }
        }
    }
}
