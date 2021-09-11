using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Serialization;

namespace NetRunner2
{
    [Serializable]
    public class GlobalSettings
    {
        public bool RunOnStartup = true;
        public string DefaultFileDialogPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        public List<UserCredentials> credentials = new List<UserCredentials>();
        public List<UserApplication> applications = new List<UserApplication>();
    }

    [Serializable]
    public class UserCredentials
    {
        public string domain, username, password;
        public bool IsSelected = false;

        public UserCredentials() { domain = ""; username = ""; password = "";}

        public UserCredentials(string domain, string username, string password) { this.domain = domain; this.username = username; this.password = password; }

        public string credentialId { get { return String.Join("/", domain, username); } }

        public override string ToString()
        {
            return String.Join(" @ ", username, domain);
        }
    }

    [Serializable]
    public class KeyboardHotkey
    {
        public Key AN = Key.None;
        public ModifierKeys Mod1 = ModifierKeys.None, Mod2 = ModifierKeys.None;

        [XmlIgnore]
        public List<string> HotKeyKeyNames
        {
            get
            {
                List<string> tmp = new List<string>();

                if (Mod1 != ModifierKeys.None) tmp.Add(Mod1.ToString());
                if (Mod2 != ModifierKeys.None) tmp.Add(Mod2.ToString());
                if (AN != Key.None) tmp.Add(AN.ToString());

                return tmp;
            }
        }
    }

    [Serializable]
    public class UserApplication
    {
        public string id, name, filepath, arguments, icon_filepath;
        public int list_order = 0;

        public KeyboardHotkey hotkey = new KeyboardHotkey();

        public UserApplication()
        {
            id = GenerateID();

            name = ""; filepath = ""; arguments = ""; icon_filepath = "";
        }

        public UserApplication(string name, string filepath, string arguments, string icon_filepath)
        {
            id = GenerateID();
            this.name = name; this.filepath = filepath; this.arguments = arguments; this.icon_filepath = icon_filepath;
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
