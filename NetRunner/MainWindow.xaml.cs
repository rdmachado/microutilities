using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NetRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool IsEditModeOn = false;
        AddProgram addform;
        public MainWindow()
        {
            InitializeComponent();

            AppManager.SettingsChanged += AppManager_SettingsChanged;
            
            Refresh();
        }

        private void AppManager_SettingsChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            panelApps.Children.Clear();
            foreach (var app in AppManager.settings.appList)
            {
                Button btn = GenerateButton(app.id, app.name, app.prog_path, app.prog_args, false);
                panelApps.Children.Add(btn);
            }
            if (IsEditModeOn)
                panelApps.Children.Add(GenerateButton("insert", "Add Program", "", "", true));
        }
        
        private Button GenerateButton(string id, string caption, string launchpath, string arguments, bool launchaddform)
        {
            Button btn = new Button();
            StackPanel stk = new StackPanel();
            Image img = new Image();
            Label lbl = new Label();

            try
            {
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(launchpath);
                img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                var uriSource = new Uri(@"/NetRunner;component/images/plus.png", UriKind.Relative);
                img.Source = new BitmapImage(uriSource);
            }

            btn.Width = 100;
            btn.Height = 100;
            btn.Name = "btn" + id;

            stk.Height = 100;
            stk.Width = 100;

            img.Height = 45;
            img.Margin = new Thickness(18, 10, 18, 10);

            lbl.HorizontalAlignment = HorizontalAlignment.Center;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            lbl.Content = caption;

            stk.Children.Add(img);
            stk.Children.Add(lbl);
            btn.Content = stk;

            if (launchaddform)
            {
                btn.Click += (s, e) =>
                {
                    if (addform == null || !addform.IsVisible)
                    {
                        addform = new AddProgram();
                        addform.Show();
                    }
                };
            }
            else
            {
                btn.Click += (s, e) =>
                {
                    if (IsEditModeOn)
                    {
                        if (addform == null || !addform.IsVisible)
                        {
                            var app = AppManager.settings.appList.Find(k => k.id == id);
                            addform = new AddProgram(app.id, app.name, app.prog_path, app.prog_args, app.AN, app.Mod1, app.Mod2);
                            addform.Show();
                        }
                    }
                    else
                    {
                        AppManager.RunProgram(launchpath, arguments);
                    }
                };
            }

            txtDomain.Text = AppManager.settings.domain;
            txtUsername.Text = AppManager.settings.username;
            txtPassword.Password = AppManager.settings.password;

            return btn;
        }

        private void ToggleEditMode(bool IsEditModeOn)
        {
            if (IsEditModeOn)
            {
                panelApps.Children.Add(GenerateButton("insert", "Add Program", "", "", true));
            }
            else
            {
                panelApps.Children.RemoveAt(panelApps.Children.Count - 1);
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsEditModeOn = !IsEditModeOn;
            ToggleEditMode(IsEditModeOn);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void cbLock_Checked(object sender, RoutedEventArgs e)
        {
            txtDomain.IsEnabled = false;
            txtUsername.IsEnabled = false;
            txtPassword.IsEnabled = false;
            btnSaveCreds.IsEnabled = false;
        }

        private void cbLock_Unchecked(object sender, RoutedEventArgs e)
        {
            txtDomain.IsEnabled = true;
            txtUsername.IsEnabled = true;
            txtPassword.IsEnabled = true;
            btnSaveCreds.IsEnabled = true;
        }

        private void btnSaveCreds_Click(object sender, RoutedEventArgs e)
        {
            AppManager.settings.domain = txtDomain.Text;
            AppManager.settings.username = txtUsername.Text;
            AppManager.settings.password = txtPassword.Password;
            AppManager.SaveSettings();
        }
    }
}
