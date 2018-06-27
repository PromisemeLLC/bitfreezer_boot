using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace TailsBoot
{
    public class Updater
    {
        public Updater()
        {
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += bgWorker_DoWork;
            _bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
        }

        private static Uri UpdateXmlLocation => new Uri("xml location on our server");

        private static string ApplicationName => "BitFreezerBoot";

        private static string ApplicationId => "BitFreezerBoot";

        private static Assembly ApplicationAssembly => Assembly.GetExecutingAssembly();

        public void DoUpdate()
        {
            Thread.Sleep(1000);
            _bgWorker.RunWorkerAsync();
        }

        private static void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!BtfUpdateXml.ExistsOnServer(UpdateXmlLocation))
            {
                e.Cancel = true;
            }
            else
            {
                e.Result = BtfUpdateXml.Parse(UpdateXmlLocation, ApplicationId);
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) return;
            var update = (BtfUpdateXml)e.Result;

            if (!update.IsNewerThen(ApplicationAssembly.GetName().Version)) return;
            if (new BtfUpdateAcceptWindow(ApplicationName, update).ShowDialog() == true)
                DownloadUpdate(update);
        }

        private void DownloadUpdate(BtfUpdateXml update)
        {
            var form = new BtfUpdateDownloadWindow(update.Uri);
            var showDialog = form.ShowDialog();

            var result = showDialog != null && (bool)showDialog;

            if (!result) return;
            var currentPath = ApplicationAssembly.Location;
            var newPath = Path.GetDirectoryName(currentPath) + "\\" + update.FileName;

            UpdateApplication(form.TempFilePath, currentPath, newPath, update.LaunchArgs);

            Environment.Exit(0);
        }

        private static void UpdateApplication(string tempFilePath, string currentPath, string newPath, string launchArgs)
        {
            var argument = "/C Choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & Choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\" & Start \"\" /D \"{3}\" \"{4}\" {5}";

            var info = new ProcessStartInfo
            {
                Arguments =
                    string.Format(argument, currentPath, tempFilePath, newPath, Path.GetDirectoryName(newPath),
                        Path.GetFileName(newPath), launchArgs),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Verb = "runas",
                FileName = "cmd.exe"
            };

            Process.Start(info);
        }

        private readonly BackgroundWorker _bgWorker;

    }
}
