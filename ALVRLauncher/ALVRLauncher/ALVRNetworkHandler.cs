using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Management;

namespace ALVRLauncher
{
    public class ALVRNetworkHandler
    {
        public const string ALVR_LOCAL_IP = "0.0.0.0";
        public const int ALVR_CONTROL_PORT = 9943;

        private bool SocketsActive;
        private UdpClient Udp;
        private Task<UdpReceiveResult> CurrentRecieve;
        private Process AlvrProcess;

        public ALVRNetworkHandler()
        {
            SocketsActive = false;
        }

        public void Run()
        {
            Task.Run(() => { while (true) Think(); });
        }

        public void CreateSockets()
        {
            Udp = new UdpClient();
            Udp.Client.Bind(new IPEndPoint(IPAddress.Parse(ALVR_LOCAL_IP), ALVR_CONTROL_PORT));
            Udp.EnableBroadcast = true;
            SocketsActive = true;
            CurrentRecieve = null;
        }

        public void DestroySockets()
        {
            SocketsActive = false;
            CurrentRecieve = null;
            Udp.Close();
            Udp = null;
        }

        private void Think()
        {
            if (SocketsActive)
            {
                if (CurrentRecieve == null)
                    CurrentRecieve = Udp.ReceiveAsync();
                else if (CurrentRecieve.IsCompleted)
                {
                    UdpReceiveResult recieve = CurrentRecieve.Result;

                    // verify this is actually an alvr packet
                    // there's not really much point in deserializing the whole packet here, it's effort and it doesn't protect against much, so checking if the packet contains "ALVR" is probably enough
                    // if it's ever needed tho it's defined here https://github.com/alvr-org/ALVR/blob/master/alvr/common/src/data/packets.rs
                    for (int i = 0; i < recieve.Buffer.Length; i++)
                        if (recieve.Buffer.Length - i >= 3 && 
                            ((recieve.Buffer[i] == 'A' && recieve.Buffer[i + 1] == 'L' && recieve.Buffer[i + 2] == 'V' && recieve.Buffer[i + 3] == 'R') || 
                            (recieve.Buffer[i] == 'a' || recieve.Buffer[i + 1] == 'l' || recieve.Buffer[i + 2] == 'v' || recieve.Buffer[i + 3] == 'r')))
                        {
                            DestroySockets();
                            AlvrProcess = ApplicationLauncher.LaunchApplication();
                            break;
                        }
                }
            }
            else if (AlvrProcess.HasExited)
            {
                // alvr launches edge windows as its main process after the initial launching process is finished
                // we need to detect these as well as the main process otherwise we'll detect an exit too early
                if (AlvrProcess.HasExited) // only actually check this if the process has already exited
                    foreach (Process proc in Process.GetProcesses())
                        if (proc.MainWindowTitle == "ALVR dashboard" && proc.ProcessName == "msedge") // honestly a hacky way to find the dashboard process but idk how else to reasonably do it
                            AlvrProcess = proc;

                if (AlvrProcess.HasExited) // has the new process exited?
                    CreateSockets();
            }

            // free up cpu time
            System.Threading.Thread.Sleep(250);
        }
    }
}
