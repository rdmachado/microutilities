using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Timers;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Media;

namespace Timer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private TimeSpan countdownSpan;
        private System.Timers.Timer timer;
        private DateTime startTime;
        private string _timeLeft;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            TimeLeft = "003000";

            timer = new System.Timers.Timer(1000);
            timer.Start();
        }

        #region Timer

        public string TimeLeft
        {
            get { return _timeLeft; }
            set { _timeLeft = value; OnPropertyChanged("TimeLeft"); }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }

        protected void StartTimer()
        {
            int hours = int.Parse(TimerCount.Text.Substring(0, 2)),
            minutes = int.Parse(TimerCount.Text.Substring(3, 2)),
            seconds = int.Parse(TimerCount.Text.Substring(6, 2));

            if (hours < 0 || minutes < 0 || minutes >= 60 || seconds < 0 || seconds >= 60)
            {
                TimeLeft = "000000";
                return;
            }
            countdownSpan = new TimeSpan(hours, minutes, seconds);
            startTime = DateTime.Now;
            timer.Elapsed += Timer_Elapsed;
            TimerCount.IsReadOnly = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var ts = countdownSpan - (DateTime.Now - startTime);
            TimeLeft = ts.ToString("hhmmss");
            if (ts <= TimeSpan.Zero)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    StopTimer(false);
                }));
            }
        }

        private void StopTimer(bool WasCanceled)
        {
            if (!WasCanceled)
            {
                PartyUp();
            }
            timer.Elapsed -= Timer_Elapsed;
            TimeLeft = "000000";
            TimerCount.IsReadOnly = false;
        }

        private void PartyUp()
        {
            Container.Topmost = true;
            Container.Topmost = checkBox.IsChecked ?? true;
            ColorAnimation colorChangeAnimation = new ColorAnimation() { From = (Color)ColorConverter.ConvertFromString("#ff0800"), To = (Color)ColorConverter.ConvertFromString("#FF9B9B9B"), Duration = new TimeSpan(0, 0, 5) };
            TimerCount.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorChangeAnimation);
            SystemSounds.Exclamation.Play();
        }

        #endregion

        #region UIEventHandlers

        private void close_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            this.Close();
        }

        private void _404Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://404vault.tumblr.com/");
        }

        private void TimerCount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TimerCount.SelectionLength = 0;
            if (e.Key == Key.Back)
            {
                e.Handled = true;
            }
        }

        private void TimerCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var asdf = e.Text;
            if (!char.IsDigit(e.Text[0]))
            {
                e.Handled = true;
            }

        }

        private void TimerCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartTimer();
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            Container.Topmost = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Container.Topmost = false;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            StopTimer(true);
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion
    }
    
}
