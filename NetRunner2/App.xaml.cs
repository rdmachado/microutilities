using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using Application = System.Windows.Forms.Application;
using System.Linq;

namespace NetRunner2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon ni;
        private MainWindow mainwindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Settings.Load();
            Settings.Save();

            #region Register Hotkeys

            Settings.instance.applications.Where(k=> k.hotkey.AN != System.Windows.Input.Key.None).ToList().ForEach(k => HotkeyManager.Instance.RegisterHotKey(k.id, k.hotkey));

            HotkeyManager.Instance.HotKeyPressed += Handle_HotKeyPressed;

            #endregion

            #region Create Tray Icon

            ni = new NotifyIcon();
            ni.MouseUp += (s, a) => { if (a.Button == MouseButtons.Left) ToggleMainWindow(); };
            ni.Icon = System.Drawing.SystemIcons.Application;

            ni.Visible = true;

            ni.ContextMenuStrip = new ContextMenuStrip();
            ni.ContextMenuStrip.Items.Add("Open").Click += (s, a) => ToggleMainWindow();

            ni.ContextMenuStrip.Items.Add($"Run on Startup: {(CheckRunOnStartup() ?  "ON" : "OFF")}").Click += (s, a) => ToggleRunOnStartup();

            ni.ContextMenuStrip.Items.Add("Close").Click += (s, a) => ShutdownApplication();

            #endregion

            if (e.Args.Length > 0 && e.Args[0] == "/show")
                ToggleMainWindow();

        }

        private void Handle_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            var application = Settings.instance.applications.Where(a => a.id == e.id).FirstOrDefault();
            var credentials = Settings.instance.credentials.Where(c => c.IsSelected).DefaultIfEmpty(new UserCredentials()).First();

            if (application != null)
                Impersonator.CreateProcessWithNetCredentials(credentials, application);
        }

        private void ToggleMainWindow() 
        {
            if (mainwindow != null && mainwindow.WindowState == WindowState.Normal && mainwindow.IsLoaded)
            {
                mainwindow.Close();
            }
            else
            {
                mainwindow = new MainWindow();

                mainwindow.WindowStartupLocation = WindowStartupLocation.Manual;

                // Find screen dpi and unitsize in case the display is scaled
                IntPtr dDC = GetDC(IntPtr.Zero);
                int dpi = GetDeviceCaps(dDC, 88);
                bool rv = ReleaseDC(IntPtr.Zero, dDC);
                double physicalUnitSize = (1d / 96d) * (double)dpi;


                var mousepos = Control.MousePosition;

                mainwindow.Show();

                var screenheight = Screen.FromPoint(mousepos).WorkingArea.Height;
                
                double X = mousepos.X / physicalUnitSize - mainwindow.ActualWidth / 2;
                double Y = Math.Min(mousepos.Y, screenheight) / physicalUnitSize - mainwindow.ActualHeight;

                mainwindow.Left = X;
                mainwindow.Top = Y;
            }
        }

        private bool CheckRunOnStartup()
        {
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            if (regkey.GetValue(Application.ProductName) != null)
                return true;
            else
                return false;
        }

        private void ToggleRunOnStartup()
        {
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (CheckRunOnStartup())
            {
                regkey.DeleteValue(Application.ProductName);
                ni.ContextMenuStrip.Items[1].Text = "Run on Startup: OFF";
            }   
            else
            {
                regkey.SetValue(Application.ProductName, Application.ExecutablePath);
                ni.ContextMenuStrip.Items[1].Text = "Run on Startup: ON";
            }   
        }

        private void ShutdownApplication() 
        {
            ni.Dispose();
            ni = null;

            HotkeyManager.Instance.UnregisterAllHotKeys();

            App.Current.Shutdown();
        }

        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

    }
}
