using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace TailsBoot
{
    /// <summary>
    /// Interaction logic for BtfUpdateDownloadWindow.xaml
    /// </summary>
    public partial class BtfUpdateDownloadWindow
    {
        private readonly WebClient _webClient;
        //private readonly BackgroundWorker _bgWorker;
        public string TempFilePath { get; }

        public BtfUpdateDownloadWindow(Uri location)
        {
            InitializeComponent();

            TempFilePath = Path.GetTempFileName();

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
            _webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;

            //_bgWorker = new BackgroundWorker();
            //_bgWorker.DoWork += bgWorker_DoWork;
            //_bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            var mw = new MessageWindow("Location: " + location + "\nTempFilePath: " + TempFilePath);
            mw.Show();

            try
            {
                _webClient.DownloadFileAsync(location, TempFilePath);
            }
            catch
            {
                DialogResult = false;
                Close();
                var mw1 = new MessageWindow("Location: " + location + "\nTempFilePath: " + TempFilePath);
                mw1.Show();
            }
        }

        //private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    //string file = ((string[])e.Argument)[0];
        //    //string updatemd5 = ((string[])e.Argument)[1];


        //    //if (Hasher.HashFile(file, HashType.MD5).ToUpper() != updatemd5)
        //    //    e.Result = DialogResult.No;
        //    //else
        //    e.Result = true;
        //}

        //private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    DialogResult = (bool)e.Result;
        //    Close();
        //}

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            lblProgress.Text =
                $"Downloaded {FormatBytes(e.BytesReceived, 1, true)} of {FormatBytes(e.TotalBytesToReceive, 1, true)}";
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                DialogResult = false;
                Close();
            }
            else if (e.Cancelled)
            {
                DialogResult = false;
                Close();
            }
            else
            {
                lblProgress.Text = "Verifying Download...";
                progressBar.IsIndeterminate = true;

                DialogResult = true;
                Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!_webClient.IsBusy) return;
            _webClient.CancelAsync();
            DialogResult = false;

            //if (!_bgWorker.IsBusy) return;
            //_bgWorker.CancelAsync();
            //DialogResult = false;
        }

        private static string FormatBytes(long bytes, int decimalPlaces, bool showByteType)
        {
            double newBytes = bytes;
            var formatString = "{0";
            string byteType;

            if (newBytes > 1024 && newBytes < 1048576)
            {
                newBytes /= 1024;
                byteType = "KB";
            }
            else if (newBytes > 1048576 && newBytes < 1073741824)
            {
                newBytes /= 1048576;
                byteType = "MB";
            }
            else
            {
                newBytes /= 1073741824;
                byteType = "GB";
            }

            if (decimalPlaces > 0)
                formatString += ":0.";

            for (var i = 0; i < decimalPlaces; i++)
            {
                formatString += "0";
            }
            formatString += "}";

            if (showByteType)
                formatString += byteType;

            return string.Format(formatString, newBytes);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private bool _clicked;
        private Point _lmAbs;

        private void BtfUpdateDownloadWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_clicked) return;
            var mousePosition = e.GetPosition(this);
            var mousePositionAbs = new Point
            {
                X = Convert.ToInt16(Left) + mousePosition.X,
                Y = Convert.ToInt16(Top) + mousePosition.Y
            };
            Left = Left + (mousePositionAbs.X - _lmAbs.X);
            Top = Top + (mousePositionAbs.Y - _lmAbs.Y);
            _lmAbs = mousePositionAbs;
        }

        private void BtfUpdateDownloadWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clicked = true;
            _lmAbs = e.GetPosition(this);
            _lmAbs.Y = Convert.ToInt16(Top) + _lmAbs.Y;
            _lmAbs.X = Convert.ToInt16(Left) + _lmAbs.X;
        }

        private void BtfUpdateDownloadWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _clicked = false;
        }
    }
}
