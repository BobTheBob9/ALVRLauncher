using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ALVRLauncher
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // create tray icon
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Text = "ALVR Automatic Launcher";
            trayIcon.Icon = new System.Drawing.Icon(SystemIcons.Application, 40, 40);

            ContextMenu trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", Stop);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            // set up our sockets
            ALVRNetworkHandler handler = new ALVRNetworkHandler();
            handler.CreateSockets();
            handler.Run();

            // start winforms app
            Application.Run();
        }

        public static void Stop(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
