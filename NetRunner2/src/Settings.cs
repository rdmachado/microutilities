using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetRunner2
{
    class Settings
    {
        public static string settingsfilepath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "settings.ini";

        public static GlobalSettings instance = new GlobalSettings();

        public static event EventHandler SettingsChanged = delegate { };

        public static void Save() { SaveSettings(instance, settingsfilepath); }
        public static void Load() { LoadSettings(ref instance, settingsfilepath); }



        private static bool SaveSettings(GlobalSettings settings, string filepath)
        {
            try
            {
                string xml = string.Empty;
                var xs = new XmlSerializer(typeof(GlobalSettings));
                using (var writer = new StringWriter())
                {
                    xs.Serialize(writer, settings);
                    writer.Flush();
                    xml = writer.ToString();
                }
                File.WriteAllText(filepath, xml);

                SettingsChanged(null, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while saving settings to disk: " + ex.Message, "NetRunner2");
                return false;
            }
        }

        private static bool LoadSettings(ref GlobalSettings settings, string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    return false;
                }

                string xml = File.ReadAllText(filepath);
                var xs = new XmlSerializer(settings.GetType());

                GlobalSettings ret = (GlobalSettings)xs.Deserialize(new StringReader(xml));

                settings = ret;
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception has occurred while loading settings from disk: " + ex.Message, "NetRunner2");
                return false;
            }

        }

    }
}
