using NetRunner2.src;
using NetRunner2.ui;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetRunner2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

#warning TODO: custom scrollbar


        public MainWindow()
        {
            InitializeComponent();

            RefreshUI();
        }

        //protected override void OnDeactivated(EventArgs e)
        //{
        //    base.OnDeactivated(e);
            
        //    if (GlobalState.Instance.editUserApplicationWindow == null)
        //        this.Close();
        //}

        private void RefreshUI()
        {
            Settings.Load();

            cmbCredentials.ItemsSource = Settings.instance.credentials;
            cmbCredentials.SelectedItem = Settings.instance.credentials.Where(e => e.IsSelected).DefaultIfEmpty(new UserCredentials()).First();

            if (Settings.instance.applications.Count == 0)
            {
                textboxEmptyAppList.Visibility = Visibility.Visible;
                stackApps.Visibility = Visibility.Hidden;
            }
            else
            {
                textboxEmptyAppList.Visibility = Visibility.Hidden;
                stackApps.Visibility = Visibility.Visible;

                stackApps.Children.Clear();

                foreach (var app in Settings.instance.applications)
                {
                    stackApps.Children.Add(new UserApplicationUI(app));
                }
            }
                
        }

        private void buttonAddApplication_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonOptions_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmbCredentials_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            lblSelectedCredentials.Content = cmbCredentials.SelectedItem.ToString();

            Settings.instance.credentials.ForEach(k => k.IsSelected = false);
            Settings.instance.credentials.Where(j => j.credentialId == ((UserCredentials)cmbCredentials.SelectedItem).credentialId).DefaultIfEmpty(new UserCredentials()).First().IsSelected = true;
            Settings.Save();
        }

        private void lblSelectedCredentials_MouseEnter(object sender, MouseEventArgs e)
        {
            lblSelectedCredentials.BorderBrush = System.Windows.Media.Brushes.LightGray;
        }

        private void lblSelectedCredentials_MouseLeave(object sender, MouseEventArgs e)
        {
            lblSelectedCredentials.BorderBrush = System.Windows.Media.Brushes.Transparent;
        }

        private void lblSelectedCredentials_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cmbCredentials.IsDropDownOpen = true;
        }
    }
}
