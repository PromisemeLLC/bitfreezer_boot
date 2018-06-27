using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace TailsBoot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {

            var rbw = new ReportBugWindow(e.Exception.ToString());
            rbw.ShowDialog();
            e.Handled = true;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var up = new Updater();
            up.DoUpdate();
            if (!CheckUEFI())
            {
                var mainWind = new MainWindow();
                mainWind.Show();
            }
            else
            {
                var secWind = new SecondMainWindow();
                secWind.Show();
            }
        }

        private bool CheckUEFI()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "bcdedit.exe",
                    Arguments = "/enum all",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    Verb = "runas"
                }
            };
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var chkUefi = false;
            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (!line.StartsWith("Firmware Boot Manager")) continue;
                chkUefi = true;
            }
            return chkUefi;
        }
    }
}
