using NetRunner2.ui;

namespace NetRunner2.src
{
    public sealed class GlobalState
    {
        private static readonly GlobalState instance = new GlobalState();

        static GlobalState() { }

        private GlobalState() { }

        public static GlobalState Instance { get { return instance; } }


        public void InvokeSettingsWindow()
        {

        }

        public void InvokeEditUserApplicationWindow()
        {

        }
        public void InvokeEditUserApplicationWindow(string applicationId)
        {

        }
    }
}
