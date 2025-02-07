﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Beanfun
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                AssemblyName assemblyName = new AssemblyName(args.Name);

                if (assemblyName.Name.EndsWith(".resources")) return null;

                string path = assemblyName.Name + ".dll";
                if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                {
                    path = string.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
                }

                using (Stream stream = executingAssembly.GetManifestResourceStream(path))
                {
                    if (stream == null)
                        return null;

                    byte[] assemblyRawBytes = new byte[stream.Length];
                    stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                    return Assembly.Load(assemblyRawBytes);
                }
            };
        }

        public static readonly Version OSVersion = Environment.OSVersion.Version;
        public static readonly Version Win2000 = new Version(5, 0);
        public static readonly Version WinXP = new Version(5, 1);
        public static readonly Version Win2003 = new Version(5, 2);
        public static readonly Version WinVista = new Version(6, 0);
        public static readonly Version Win7 = new Version(6, 1);
        public static readonly Version Win8 = new Version(6, 2);
        public static readonly Version Win8_1 = new Version(6, 3);
        public static readonly Version Win10 = new Version(10, 0);
        public static readonly Version Win11 = new Version(10, 0, 22000, 0);

        public static MainWindow MainWnd {
            get
            {
                Window wnd = Current.MainWindow;
                if (wnd != null && (typeof(MainWindow) == wnd.GetType()))
                    return (MainWindow) wnd;
                else
                    return null;
            }
        }

        public static string LoginRegion = ConfigAppSettings.GetValue("loginRegion", "TW");
        public static int LoginMethod = int.Parse(ConfigAppSettings.GetValue("loginMethod", "0"));
        private void Main(object sender, StartupEventArgs e)
        {
            WindowsAPI.AttachConsole(-1);

            I18n.LoadLanguage(ConfigAppSettings.GetValue("Language", null));

            StartupUri = new Uri("MainWindow.xaml", UriKind.RelativeOrAbsolute);
        }

        public bool compareFile(string path1, string path2)
        {
            var hash = System.Security.Cryptography.HashAlgorithm.Create();
            var stream_1 = File.OpenRead(path1);
            byte[] hashByte_1 = hash.ComputeHash(stream_1);
            stream_1.Close();

            var stream_2 = File.OpenRead(path2);
            byte[] hashByte_2 = hash.ComputeHash(stream_2);
            stream_2.Close();

            return BitConverter.ToString(hashByte_1) == BitConverter.ToString(hashByte_2);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (MainWnd != null && MainWnd.bfClient != null) try { MainWnd.bfClient.Logout(); } catch { }

            Process[] processes = Process.GetProcessesByName("BFWidgetKernel");
            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch { }
            }
        }

        public static string ConvertVersion(Version version)
        {
            if (version < new Version(4,1))
                return $"{version.Major}.{version.Minor}.{version.Build}({version.Revision})";
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
            return $"{version.Major}.{version.Minor}({buildDate.ToString("yyMMddHHmm")})";
        }

        internal static string AssemblyVersion
        {
            get
            {
                return ConvertVersion(Assembly.GetExecutingAssembly().GetName().Version);
            }
        }
    }
}
