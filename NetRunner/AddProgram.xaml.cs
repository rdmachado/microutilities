using System;
using System.Windows;
using System.Windows.Input;

namespace NetRunner
{
    /// <summary>
    /// Interaction logic for AddProgram.xaml
    /// </summary>
    public partial class AddProgram : Window
    {
        bool IsEditMode = false;
        string appid = "";
        Key AN = Key.None;
        ModifierKeys Mod1 = ModifierKeys.None;
        ModifierKeys Mod2 = ModifierKeys.None;
        
        public AddProgram()
        {
            InitializeComponent();
        }
        public AddProgram(string id, string ProgramName, string ProgramPath, string ProgramArgs, Key AN, ModifierKeys Mod1, ModifierKeys Mod2)
        {
            InitializeComponent();
            IsEditMode = true;
            appid = id;
            btnDelete.IsEnabled = true;
            this.Title = "Edit";
            txtProgramName.Text = ProgramName;
            txtProgramPath.Text = ProgramPath;
            txtProgramArgs.Text = ProgramArgs;
            this.AN = AN;
            txtANKey.Text = AN.ToString();
            this.Mod1 = Mod1;
            txtModKey1.Text = Mod1.ToString();
            this.Mod2 = Mod2;
            txtModKey2.Text = Mod2.ToString();
        }
        

        Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            fd.Filter = "Executables |*.exe|Batch files|*.bat";
            fd.DefaultExt = ".exe";
            fd.InitialDirectory = "C:\\";
            fd.FileOk += Fd_FileOk;
            fd.ShowDialog();
        }

        private void Fd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtProgramPath.Text = fd.FileName;
            if (String.IsNullOrEmpty(txtProgramName.Text))
                txtProgramName.Text = fd.SafeFileName;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (AN == Key.None || Mod1 == Mod2)
            {
                AN = Key.None; Mod1 = ModifierKeys.None; Mod2 = ModifierKeys.None;
            }

            if (IsEditMode)
            {
                AppManager.EditProgram(appid, txtProgramName.Text, txtProgramPath.Text, txtProgramArgs.Text, "", AN, Mod1, Mod2);
            }
            else
            {
                AppManager.AddProgram(txtProgramName.Text, txtProgramPath.Text, txtProgramArgs.Text, "", AN, Mod1, Mod2);
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            AppManager.DeleteProgram(appid);
            this.Close();
        }

        private void txtModKey1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key == Key.System ? e.SystemKey : e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    txtModKey1.Text = ModifierKeys.Control.ToString();
                    Mod1 = ModifierKeys.Control;
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    txtModKey1.Text = ModifierKeys.Shift.ToString();
                    Mod1 = ModifierKeys.Shift;
                    break;
                case Key.LeftAlt:
                case Key.RightAlt:
                    txtModKey1.Text = ModifierKeys.Alt.ToString();
                    Mod1 = ModifierKeys.Alt;
                    break;
                default:
                    txtModKey1.Text = "";
                    Mod1 = ModifierKeys.None;
                    break;
            }

            if (txtModKey1.Text == txtModKey2.Text)
            {
                txtModKey2.Text = "";
                Mod2 = ModifierKeys.None;
            }                

            txtModKey2.Focus();
            e.Handled = true;
        }

        private void txtModKey2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key == Key.System ? e.SystemKey : e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    txtModKey2.Text = ModifierKeys.Control.ToString();
                    Mod2 = ModifierKeys.Control;
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    txtModKey2.Text = ModifierKeys.Shift.ToString();
                    Mod2 = ModifierKeys.Shift;
                    break;
                case Key.LeftAlt:
                case Key.RightAlt:
                    txtModKey2.Text = ModifierKeys.Alt.ToString();
                    Mod2 = ModifierKeys.Alt;
                    break;
                default:
                    txtModKey2.Text = "";
                    Mod2 = ModifierKeys.None;
                    break;
            }

            if (txtModKey1.Text == txtModKey2.Text)
            {
                txtModKey2.Text = "";
                Mod2 = ModifierKeys.None;
            }                

            txtANKey.Focus();
            e.Handled = true;
        }

        private void txtANKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                txtANKey.Text = Enum.GetName(typeof(Key), e.Key).ToUpper();
                AN = e.Key;
            }

            e.Handled = true;
        }
    }
}
