using Microsoft.Win32;
using System;
using System.Windows;

namespace NetRunner2.ui
{
    /// <summary>
    /// Interaction logic for EditUserApplication.xaml
    /// </summary>
    public partial class EditUserApplication : Window
    {
#warning TODO: Implement hotkey

        OpenFileDialog openFileDialog;
        string appID;

        public EditUserApplication()
        {
            InitializeComponent();
        }

        //protected override void OnDeactivated(EventArgs e)
        //{
        //    base.OnDeactivated(e);
        //    this.Close();
        //}

        public EditUserApplication(string appID)
        {
            InitializeComponent();

            this.appID = appID;

            UserApplication userApplication = Settings.instance.applications.Find(e => e.id == appID);
            textboxName.Text = userApplication.name;
            textboxPath.Text = userApplication.filepath;
            textboxArguments.Text = userApplication.arguments;
        }

        private void buttonOpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executables |*.exe|Batch files|*.bat";
            openFileDialog.DefaultExt = ".exe";
            openFileDialog.InitialDirectory = Settings.instance.DefaultFileDialogPath;
            openFileDialog.FileOk += OpenFileDialog_FileOk;
            openFileDialog.ShowDialog();
        }

        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            textboxPath.Text = openFileDialog.FileName;
            if (String.IsNullOrEmpty(textboxPath.Text))
                textboxPath.Text = openFileDialog.SafeFileName;
        }

        private void buttonAddApplication_Click(object sender, RoutedEventArgs e)
        {
            if (appID != null)
            {
                UserApplication userApplication = Settings.instance.applications.Find(a => a.id == appID);
                userApplication.name = textboxName.Text;
                userApplication.filepath = textboxPath.Text;
                userApplication.arguments = textboxArguments.Text;
            }
            else
            {
                Settings.instance.applications.Add(new UserApplication(
                    textboxName.Text,
                    textboxPath.Text,
                    textboxArguments.Text,
                    ""
                    ));
            }

            Settings.Save();

            this.Close();
        }
    }
}
