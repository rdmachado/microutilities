using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ShiftTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            App.Current.Properties["closetimer"] = 4;
            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "/keepopen")
                    App.Current.Properties["closetimer"] = -10;
            }
            if (e.Args.Length == 2)
                App.Current.Properties["initfile"] = e.Args[1];

            MainWindow wnd = new MainWindow();
            wnd.Show();
        }
    }
}
