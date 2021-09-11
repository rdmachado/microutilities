using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NHotkey;
using NHotkey.Wpf;
using System.Xml.Serialization;
using System.Windows.Input;
using System.Diagnostics;

namespace NetRunner
{
    public static class AppManager
    {
        public static event EventHandler SettingsChanged = delegate { };

        public static int NextAvailablePosition
        {
            get { return settings.appList.Count > 0 ? settings.appList.Max(e => e.order) + 1 : 0; }
        }

        private static string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\netrunner-settings.xml";
        private static string shifttrackerSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\shifttracker-settings.xml";
        
        public static UserSettings settings;

        public static void SaveSettings()
        {
            try
            {
                string xml = string.Empty;
                var xs = new XmlSerializer(typeof(UserSettings));
                using (var writer = new StringWriter())
                {
                    xs.Serialize(writer, settings);
                    writer.Flush();
                    xml = writer.ToString();
                }
                File.WriteAllText(settingsPath, xml);
                File.WriteAllText(shifttrackerSettingsPath, String.Join("\n", settings.username, settings.password));

                UnregisterAllHotkeys(settings.appList);
                RegisterAllHotkeys(settings.appList);

                SettingsChanged(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while saving settings to disk: " + ex.Message, "NetRunner");
            }
            
        }
        
        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsPath))
                {
                    settings = new UserSettings() { domain = "domain", username = "user", password = "pass" };
                    return;
                }

                string xml = File.ReadAllText(settingsPath);
                var xs = new XmlSerializer(typeof(UserSettings));

                UserSettings ret = (UserSettings)xs.Deserialize(new StringReader(xml));

                UnregisterAllHotkeys(ret.appList);
                RegisterAllHotkeys(ret.appList);

                settings = ret;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while loading settings from disk: " + ex.Message, "NetRunner");
            }
            
        }

        private static void RegisterAllHotkeys(List<Program> appList)
        {
            try
            {
                appList.ForEach(e => RegisterHotkey(e.id, e.AN, e.Mod1, e.Mod2));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while registering one or more hotkeys: " + ex.Message, "NetRunner");
            }
        }

        private static void RegisterHotkey(String id, Key AN, ModifierKeys Mod1, ModifierKeys Mod2)
        {
            try
            {
                if (AN != Key.None)
                    HotkeyManager.Current.AddOrReplace(id, AN, Mod1 | Mod2, HandleHotkeyEvent);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while registering a hotkey: " + ex.Message, "NetRunner");
            }
            
        }

        public static void UnregisterAllHotkeys(List<Program> appList)
        {
            try
            {
                foreach (var id in appList.Select(e => e.id).ToList())
                {
                    HotkeyManager.Current.Remove(id);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while unregistering one or more hotkeys: " + ex.Message, "NetRunner");
            }
        }

        private static void UnregisterHotkey(string id)
        {
            try
            {
                HotkeyManager.Current.Remove(id);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while unregistering a hotkey: " + ex.Message, "NetRunner");
            }
        }

        public static void HandleHotkeyEvent(object sender, HotkeyEventArgs e)
        {
            var app = settings.appList.Find(k => k.id == e.Name);
            RunProgram(app.prog_path, app.prog_args);
        }

        public static void RunProgram(string launchpath, string arguments)
        {
            Impersonator.CreateProcessWithNetCredentials(settings.username, settings.domain, settings.password, launchpath, arguments);
        }
        public static void RunProgramCPAU(string launchpath, string arguments)
        {
            string path = Path.Combine(Path.GetTempPath(), "queres_e_pau.exe");
            try
            {
                
                if (File.Exists(path))
                    File.Delete(path);
                File.WriteAllBytes(path, Properties.Resources.CPAU);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("A fatal exception has occurred while unpacking CPAU to \"" + Path.GetTempPath() + "\" : \n" + ex.Message, "NetRunner");
                throw ex;
            }

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.Arguments = "-u " + settings.domain + "\\" + settings.username + " -p " + settings.password + " -ex \"" + launchpath + (arguments.Length > 0 ? " \"" + arguments + "\"" : "\"");
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(launchpath);
                p.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("A fatal exception has occurred while invoking CPAU: " + ex.Message, "NetRunner");
                throw ex;
            }
            
        }

        public static void AddProgram(string name, string prog_path, string prog_args, string icon_path, Key AN, ModifierKeys Mod1, ModifierKeys Mod2)
        {
            settings.appList.Add(new Program(name, prog_path, prog_args, icon_path, NextAvailablePosition, AN, Mod1, Mod2));
            SaveSettings();
        }

        public static void EditProgram(string id, string name, string prog_path, string prog_args, string icon_path, Key AN, ModifierKeys Mod1, ModifierKeys Mod2)
        {
            var app = settings.appList.Single(e => e.id == id);
            app.name = name;
            app.prog_path = prog_path;
            app.prog_args = prog_args;
            app.icon_path = icon_path;
            app.AN = AN;
            app.Mod1 = Mod1;
            app.Mod2 = Mod2;
            SaveSettings();
        }

        public static void DeleteProgram(string id)
        {
            var app = settings.appList.Single(e => e.id == id);
            UnregisterHotkey(app.id);
            settings.appList.Remove(app);
            SaveSettings();
        }
        
    }

    [Serializable]
    public class UserSettings
    {
        public string username = "", password = "", domain = "";
        
        public List<Program> appList;

        public UserSettings()
        {
            appList = new List<NetRunner.Program>();
        }
    }

    [Serializable]
    public class Program
    {
        public string id;
        public string name = "", prog_path = "", prog_args = "", icon_path = "";
        public int order = 0;
        public Key AN = Key.None;
        public ModifierKeys Mod1 = ModifierKeys.None, Mod2 = ModifierKeys.None;

        public Program()
        {
            id = GenerateID();
        }

        public Program(string name, string prog_path, string prog_args, string icon_path, int order, Key AN, ModifierKeys Mod1, ModifierKeys Mod2)
        {
            id = GenerateID();
            this.name = name; this.prog_path = prog_path; this.prog_args = prog_args; this.icon_path = icon_path; this.order = order;
            this.AN = AN; this.Mod1 = Mod1; this.Mod2 = Mod2;
        }

        private string GenerateID()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
    }
}
