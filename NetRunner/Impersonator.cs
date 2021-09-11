using System;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;

namespace NetRunner
{
    public static class Impersonator
    {
        #region Public Methods

        public static int CreateProcessWithNetCredentials(string username, string domain, string password, string path, string command)
        {
            int ret = 0;

            StartupInfo si = new StartupInfo();
            ProcessInformation pi;

            CreateProcessWithLogonW(username, domain, password, LogonFlags.LOGON_NETCREDENTIALS_ONLY,
                path, " " + command,
                CreationFlags.CREATE_UNICODE_ENVIRONMENT,
                (UInt32)0,
                Path.GetDirectoryName(path),
                ref si,
                out pi);

            return ret;
        }

        #endregion

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StartupInfo
        {
            public int cb;
            public String reserved;
            public String desktop;
            public String title;
            public int x;
            public int y;
            public int xSize;
            public int ySize;
            public int xCountChars;
            public int yCountChars;
            public int fillAttribute;
            public int flags;
            public UInt16 showWindow;
            public UInt16 reserved2;
            public byte reserved3;
            public IntPtr stdInput;
            public IntPtr stdOutput;
            public IntPtr stdError;
        }

        [Flags]
        private enum CreationFlags
        {
            CREATE_SUSPENDED = 0x00000004,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        }

        [Flags]
        private enum LogonFlags
        {
            LOGON_WITH_PROFILE = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY = 0x00000002
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithLogonW(
           String userName,
           String domain,
           String password,
           LogonFlags logonFlags,
           String applicationName,
           String commandLine,
           CreationFlags creationFlags,
           UInt32 environment,
           String currentDirectory,
           ref StartupInfo startupInfo,
           out ProcessInformation processInformation);
    }



}
