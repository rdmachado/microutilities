using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace NetRunner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon ni;
        private MainWindow main;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppManager.LoadSettings();

            main = new MainWindow();

            ni = new NotifyIcon();
            ni.DoubleClick += (s, a) => main.Show();
            ni.Icon = System.Drawing.SystemIcons.Application;
            ni.Visible = true;

            ni.ContextMenuStrip = new ContextMenuStrip();
            ni.ContextMenuStrip.Items.Add("Open").Click += (s, a) => OpenMain();
            ni.ContextMenuStrip.Items.Add("Close").Click += (s, a) => CloseApp();

            if (e.Args.Length > 0 && e.Args[0] == "/max")
            {
                main.Show();
            }
        }

        private void OpenMain()
        {
            if (main.IsVisible)
            {
                if (main.WindowState == WindowState.Minimized)
                    main.WindowState = WindowState.Normal;

                main.Activate();
            }
            else
                main.Show();
        }

        private void CloseApp()
        {
            ni.Dispose();
            ni = null;
            AppManager.UnregisterAllHotkeys(AppManager.settings.appList);
            App.Current.Shutdown();
        }
    }
}
