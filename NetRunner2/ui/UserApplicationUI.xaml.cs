using NetRunner2.src;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NetRunner2.ui
{
    /// <summary>
    /// Interaction logic for UserApplicationUI.xaml
    /// </summary>
    public partial class UserApplicationUI : UserControl
    {
        UserApplication application;

        public UserApplicationUI(UserApplication application)
        {
            InitializeComponent();

            this.application = application;

            textboxName.Content = application.name;

            System.Drawing.Icon icon = System.Drawing.SystemIcons.Application;
            
            if (File.Exists(application.filepath))
            {
                icon = System.Drawing.Icon.ExtractAssociatedIcon(application.filepath);
            }

            System.Drawing.Bitmap imgtest = new Icon(icon, 96, 96).ToBitmap();

            imageIcon.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(imgtest.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            textboxHotkey.Content = String.Join(" + ", application.hotkey.HotKeyKeyNames);
        }

        private void OpenAppSettings()
        {
            GlobalState.Instance.InvokeEditUserApplicationWindow(application.id);
        }

        private void LaunchApp()
        {
            Impersonator.CreateProcessWithNetCredentials(Settings.instance.credentials.Where(e => e.IsSelected).DefaultIfEmpty(new UserCredentials()).First(), application);
        }

        #region Event Handlers


        private void lblEllipsis_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenAppSettings();
        }

        private void imageIcon_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LaunchApp();
        }

        private void textboxName_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LaunchApp();
        }

        private void textboxHotkey_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LaunchApp();
        }

        private void lblEllipsis_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lblEllipsis.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void lblEllipsis_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lblEllipsis.Background = System.Windows.Media.Brushes.LightGray;
        }

        private void gridMain_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gridMain.Background = System.Windows.Media.Brushes.Beige;
        }

        private void gridMain_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gridMain.Background = System.Windows.Media.Brushes.Transparent;
        }

        #endregion
    }
}
