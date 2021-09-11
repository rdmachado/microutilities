using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ShiftTracker
{
    public class WorkDay
    {
        public DateTime Date;
        public TimeSpan WorkedHours;
        public DateTime StartTime;

        public TimeSpan Balance
        {
            get { return WorkedHours.Subtract(new TimeSpan(8, 0, 0)); }
        }

        public string DateString
        {
            get { return Date.ToString("dd/MM/yyyy"); }
        }
        public string WorkedHoursString
        {
            get { return WorkedHours.ToString(@"hh\:mm\:ss"); }
        }
        public string BalanceString
        {
            get { var tmp = WorkedHours.Subtract(new TimeSpan(8, 0, 0)); return tmp.ToString((tmp < TimeSpan.Zero ? "\\-" : "\\+") + @"hh\:mm\:ss"); }
        }

        public WorkDay() { }

        public WorkDay(DateTime Date) { this.Date = Date; }
    }

    public static class TimeSpanExtensions
    {
        public static TimeSpan RoundDownToHalfHour(this TimeSpan input)
        {
            return new TimeSpan(0, (int)input.TotalMinutes - (int)input.TotalMinutes % 30, 0);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SolidColorBrush positiveBrush = new SolidColorBrush(Color.FromArgb(255, 28, 141, 33)),
                        negativeBrush = new SolidColorBrush(Color.FromArgb(255, 149, 53, 53)),
                        neutralBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        List<WorkDay> workDays = new List<WorkDay>();

        public MainWindow()
        {
            InitializeComponent();

            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);

            if (App.Current.Properties["initfile"] != null)
                ProcessXLSShiftFile((string)App.Current.Properties["initfile"]);

            if (CalculateTime())
            {
                UpdateTimeLabels();
                timer.Start();
            }
            else
            {
                UpdateTimeLabels();
            }

            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;

            ttGrid.ItemsSource = workDays;
        }

        #region Events

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan tmptsMonthly = TimeSpan.Parse((string)lblDeltaTimeMonthly.Content).Add(new TimeSpan(0, 0, 1));

            TimeSpan tmptsDay = TimeSpan.Parse((string)lblDeltaTimeDay.Content).Add(new TimeSpan(0, 0, 1));
            
            if (tmptsDay > TimeSpan.Zero)
            {
                TimeSpan tmptsDiff = TimeSpan.Parse(((string)lblDiff.Content).Replace("+", "")).Add(new TimeSpan(0, 0, 1));

                lblDiff.Content = tmptsDiff.ToString((tmptsDiff < TimeSpan.Zero ? "\\-" : "\\+") + @"hh\:mm\:ss");
                lblDiff.Foreground = ((string)lblDiff.Content) == "00:00:00" ? neutralBrush : ((string)lblDiff.Content)[0] == '-' ? negativeBrush : positiveBrush;
            }

            lblDeltaTimeMonthly.Content = tmptsMonthly.ToString((tmptsMonthly < TimeSpan.Zero ? "\\-" : "") + @"hh\:mm\:ss");
            lblDeltaTimeMonthly.Foreground = ((string)lblDeltaTimeMonthly.Content) == "00:00:00" ? neutralBrush : ((string)lblDeltaTimeMonthly.Content)[0] == '-' ? negativeBrush : positiveBrush;

            lblDeltaTimeDay.Content = (bool)App.Current.Properties["suppresstoday"] ? "00:00:00" : tmptsDay.ToString((tmptsDay < TimeSpan.Zero ? "\\-" : "") + @"hh\:mm\:ss");
            lblDeltaTimeDay.Foreground = ((string)lblDeltaTimeDay.Content) == "00:00:00" ? neutralBrush : ((string)lblDeltaTimeDay.Content)[0] == '-' ? negativeBrush : positiveBrush;

            if ((int)App.Current.Properties["closetimer"] != -10)
            {
                if ((int)App.Current.Properties["closetimer"] < 0)
                    Quit();
                else
                    App.Current.Properties["closetimer"] = (int)App.Current.Properties["closetimer"] - 1;
            }
        }

        private void LblDeltaTime_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // if multiple files were dropped get only the first one
                string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                ProcessXLSShiftFile(file);
            }

            if (CalculateTime())
            {
                UpdateTimeLabels();
                ttGrid.ItemsSource = null;
                ttGrid.ItemsSource = workDays;

#warning TODO: DYNAMIC TTGRID STYLING

                timer.Start();
            }
            else
            {
                UpdateTimeLabels();
            }

        }

        private void LblDeltaTime_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void BtnimgClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Quit();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion

        #region Inner Workings

        private void UpdateTimeLabels()
        {
            TimeSpan tmptsMonthly = (TimeSpan)App.Current.Properties["monthlybalance"];

            TimeSpan tmptsDay = (TimeSpan)App.Current.Properties["dailybalance"];

            TimeSpan tmptsDiff = (TimeSpan)App.Current.Properties["monthlyaccumulated"];

            //if (tmptsDay > TimeSpan.Zero) { tmptsDiff =  tmptsDiff.Add(tmptsDay); }

            lblDeltaTimeMonthly.Content = tmptsMonthly.ToString((tmptsMonthly < TimeSpan.Zero ? "\\-" : "") + @"hh\:mm\:ss");

            lblDeltaTimeDay.Content = (bool)App.Current.Properties["suppresstoday"] ? "00:00:00" : tmptsDay.ToString((tmptsDay < TimeSpan.Zero ? "\\-" : "") + @"hh\:mm\:ss");

            lblDiff.Content = tmptsDiff.ToString((tmptsDiff < TimeSpan.Zero ? "\\-" : "\\+") + @"hh\:mm\:ss");


            lblDeltaTimeMonthly.Foreground = ((string)lblDeltaTimeMonthly.Content) == "00:00:00" ? neutralBrush : ((string)lblDeltaTimeMonthly.Content)[0] == '-' ? negativeBrush : positiveBrush;

            lblDeltaTimeDay.Foreground = ((string)lblDeltaTimeDay.Content) == "00:00:00" ? neutralBrush : ((string)lblDeltaTimeDay.Content)[0] == '-' ? negativeBrush : positiveBrush;

            lblDiff.Foreground = ((string)lblDiff.Content) == "00:00:00" ? neutralBrush : ((string)lblDiff.Content)[0] == '-' ? negativeBrush : positiveBrush;
        }

        private void Quit()
        {
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Save();

            App.Current.Shutdown();
        }

        private void ProcessXLSShiftFile(string file)
        {
            SQLiteConnection con = new SQLiteConnection("Data Source=.\\shift.db;Version=3;");
            con.Open();

            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM clockdata;DELETE FROM dailyhours;", con);
            cmd.ExecuteNonQuery();

            foreach (string line in File.ReadAllLines(file, Encoding.Unicode).Skip(1))
            {
                if (line.Trim().Length > 0)
                {
                    string[] cols = line.Split('\t');
                    DateTime timestamp = DateTime.ParseExact(cols[0] + "T" + cols[1], "dd/MM/yyyyTHH:mm:ss", null);
                    bool is_entry = cols[2] == "E" ? true : false;
                    string exit_reason = cols[3].Trim();

                    cmd.CommandText = "INSERT INTO clockdata (clock_timestamp, is_entry, exit_reason) VALUES ('" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "', " + is_entry + ", '" + exit_reason + "');";
                    cmd.ExecuteNonQuery();
                }
            }

            con.Close();
        }

        private bool CalculateTime()
        {
            SQLiteConnection con = new SQLiteConnection("Data Source=.\\shift.db;Version=3;");
            con.Open();

            SQLiteCommand cmd = new SQLiteCommand("SELECT clock_timestamp, is_entry, exit_reason FROM clockdata WHERE SUBSTR(clock_timestamp, 0, 8) = (SELECT MAX(SUBSTR(clock_timestamp, 0, 8)) FROM clockdata) ORDER BY clock_timestamp;", con),
                         cmdi = new SQLiteCommand("INSERT INTO dailyhours (date, worked_minutes, worked_minutes_rounded) VALUES (?, ?, ?)", con);

            workDays = new List<WorkDay>();

            //

            DateTime openshiftstart = DateTime.MinValue;
            DateTime lastshiftend = DateTime.MinValue;
            TimeSpan dailyworkedhours = new TimeSpan();
            TimeSpan dailybalance = new TimeSpan();
            TimeSpan monthlybalance = new TimeSpan();
            TimeSpan monthlyaccumulated = new TimeSpan();
            TimeSpan dailymaxbreaktime = new TimeSpan();
            bool trackopenshift = false;
            bool skipnextrow = false;


            WorkDay wd = new WorkDay();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var tmpdt = reader.GetDateTime(0);
                var tmpentry = reader.GetBoolean(1);
                var tmpreason = reader.GetString(2);

                // ignorar saídas em serviço
                if (tmpreason.Length != 0 || skipnextrow)
                {
                    skipnextrow = !skipnextrow;
                    continue;
                }

                if (openshiftstart != DateTime.MinValue && tmpdt.Date != openshiftstart.Date) // novo dia
                {
                    // cobrar hora de almoço   ---- se existir uma saída inferior a 1h ou se saiu só depois das 15, mas apenas se entrou antes das 11
                    if (wd.StartTime.Hour < 11 && lastshiftend.Hour >= 15 && dailymaxbreaktime.TotalHours < 1)
                    {
                        wd.WorkedHours = dailyworkedhours.Add(dailymaxbreaktime.Subtract(new TimeSpan(1, 0, 0)));
                    }
                    else
                    {
                        wd.WorkedHours = dailyworkedhours;
                    }

                    workDays.Add(wd);

                    lastshiftend = DateTime.MinValue;
                    dailyworkedhours = new TimeSpan();
                    dailymaxbreaktime = new TimeSpan();
                }

                if (tmpentry) // entrada
                {
                    if (lastshiftend != DateTime.MinValue) // saiu e entrou no mesmo dia
                    {
                        if (dailymaxbreaktime == TimeSpan.Zero)
                        {
                            dailymaxbreaktime = tmpdt.Subtract(lastshiftend);
                        }
                    }
                    else
                    {
                        wd = new WorkDay(tmpdt.Date);
                        wd.StartTime = tmpdt;
                    }
                    openshiftstart = tmpdt;
                    trackopenshift = true;
                }
                else // saída
                {
                    lastshiftend = tmpdt;
                    dailyworkedhours = dailyworkedhours.Add(tmpdt.Subtract(openshiftstart));
                    trackopenshift = false;
                }

            }

            reader.Close();

            if (openshiftstart != DateTime.MinValue)
            {
                // add up final day
                if (trackopenshift) // open day
                {
                    dailyworkedhours = dailyworkedhours.Add(DateTime.Now.Subtract(openshiftstart));

                    // cobrar hora de almoço   ---- se existir uma saída inferior a 1h, mas apenas se entrou antes das 11
                    if (wd.StartTime.Hour < 11 && dailymaxbreaktime.TotalHours < 1 && (dailymaxbreaktime.TotalHours > 0 || DateTime.Now.Hour >= 15))
                    {
                        dailyworkedhours = dailyworkedhours.Add(dailymaxbreaktime.Subtract(new TimeSpan(1, 0, 0)));
                    }
                }
                else
                {
                    // cobrar hora de almoço   ---- se existir uma saída inferior a 1h ou se saiu só depois das 15, mas apenas se entrou antes das 11
                    if (wd.StartTime.Hour < 11 && lastshiftend.Hour >= 15 && dailymaxbreaktime.TotalHours < 1)
                    {
                        dailyworkedhours = dailyworkedhours.Add(dailymaxbreaktime.Subtract(new TimeSpan(1, 0, 0)));
                    }
                }
                wd.WorkedHours = dailyworkedhours;


                workDays.Add(wd);
            }

            if (workDays.Count > 0)
            {
                foreach (WorkDay w in workDays)
                {
                    cmdi.Parameters.Clear();
                    cmdi.Parameters.AddWithValue("date", w.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdi.Parameters.AddWithValue("workedminutes", Math.Round(w.WorkedHours.TotalMinutes, 6));
                    cmdi.Parameters.AddWithValue("workedminutesrounded", w.WorkedHours.RoundDownToHalfHour().TotalMinutes);
                    cmdi.ExecuteNonQuery();
                }

                con.Close();

                monthlybalance = TimeSpan.FromMilliseconds(workDays.Sum(e => e.Balance.TotalMilliseconds));

                WorkDay currentDay = workDays.SingleOrDefault(e => e.Date == DateTime.Now.Date);
                dailybalance = currentDay?.Balance ?? TimeSpan.Zero;

                monthlyaccumulated = TimeSpan.FromMilliseconds(workDays.Take(workDays.Count - 1).Sum(e => e.Balance.TotalMilliseconds)).Add(workDays.Last().Balance > TimeSpan.Zero ? workDays.Last().Balance : TimeSpan.Zero);
            }

            App.Current.Properties["monthlybalance"] = monthlybalance;

            App.Current.Properties["dailybalance"] = dailybalance;

            App.Current.Properties["monthlyaccumulated"] = monthlyaccumulated;

            App.Current.Properties["suppresstoday"] = dailybalance == TimeSpan.Zero;

            return dailybalance != TimeSpan.Zero;
        }

        #endregion
        
    }
}