using System;
//using SharpOSC;
using Rug.Osc;
using BuildSoft.VRChat.Osc;
using System.Threading;
using System.Net;

namespace VRCTTS
{
    class OSC
    {
        OscSender sender;
        OscReceiver receiver;

        private static Menu_Azure menuAzure;
        private static Globals globals;

        Thread oscReceiverThread;

        public OSC(Menu_Azure azure)
        {
            globals = Globals.Instance;
            menuAzure = azure;
        }

        /// <summary>
        /// Recieve OSC from VRC.
        /// </summary> 
        public void Listen()
        {
            receiver = new OscReceiver(globals.osc_port_reciever);
            oscReceiverThread = new Thread(new ThreadStart(ListenLoop));
            receiver.Connect();
            oscReceiverThread.Start();
        }

        private void ListenLoop() 
        {
            try
            {
                while (receiver.State != OscSocketState.Closed)
                {
                    if (receiver.State == OscSocketState.Connected) 
                    {
                        OscPacket packet = receiver.Receive();
                        string[] address = packet.ToString().Split(',');
                        if (address[0].Equals(globals.osc_parameter) & address[1].Equals(" True")) 
                        {
                            menuAzure.recevied_OSC = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //TODO: add log here
            }
        }

        /// <summary>
        /// Send OSC message. 
        /// </summary>
        /// <param name="message">Example: Vrchat/Parameters/Voice, False</param>
        public void sendMessage(OscMessage message)
        {
            try
            {
                using (sender = new OscSender(IPAddress.Parse(globals.osc_IPAddress), globals.osc_port_sender))
                {
                    sender.Connect();

                    sender.Send(message);
                }
                sender.Close();
            }
            catch (Exception)
            {
                throw;
                // TODO: Add log here
            }
        }

        public void stopListening() 
        {
            receiver.Close();
            oscReceiverThread.Join();
        }
    }
}
