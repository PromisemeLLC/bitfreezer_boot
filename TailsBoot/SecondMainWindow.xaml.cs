using System.Diagnostics;
using System.Windows;
using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;


namespace TailsBoot
{
    /// <summary>
    /// Interaction logic for SecondMainWindow.xaml
    /// </summary>
    public partial class SecondMainWindow
    {
        public SecondMainWindow()
        {
            InitializeComponent();
            CheckRemoveble();
            if (btfDriveIsUp)
                LogSuccessfull();
            else
                LogFault();
        }

        private bool btfDriveIsUp;

        private void LogSuccessfull()
        {

            lblTop.Text = "Bitfreezer plugged in";
            lblBot.Text = "Do not remove it";
            btnRestart.IsEnabled = true;
            var greenLFB = new LinearGradientBrush();

            greenLFB.StartPoint = new Point(0, 0);

            greenLFB.EndPoint = new Point(1, 1);
            var greenGS = new GradientStop
            {
                Color = (Color)ColorConverter.ConvertFromString("#FF00FF0D"),
                Offset = 0
            };
            greenLFB.GradientStops.Add(greenGS);

            var newGreenGS = new GradientStop
            {
                Color = (Color)ColorConverter.ConvertFromString("#FF0B7E11"),
                Offset = 1
            };
            greenLFB.GradientStops.Add(newGreenGS);
            indicator.Fill = greenLFB;
        }

        private void LogFault()
        {
            lblTop.Text = "Could not find Bitfreezer";
            lblBot.Text = "Please plug in Bitfreezer";
            btnRestart.IsEnabled = false;
            var redLFB = new LinearGradientBrush();

            redLFB.StartPoint = new Point(0, 0);

            redLFB.EndPoint = new Point(1, 1);
            var redGS = new GradientStop
            {
                Color = Colors.Red,
                Offset = 0
            };
            redLFB.GradientStops.Add(redGS);

            var newRedGS = new GradientStop
            {
                Color = (Color)ColorConverter.ConvertFromString("#FF0B7E11"),
                Offset = 1
            };
            redLFB.GradientStops.Add(newRedGS);
            indicator.Fill = redLFB;
        }

        private void CheckRemoveble()
        {
            btfDriveIsUp = false;
            foreach (var inf in DriveInfo.GetDrives())
            {
                var checkTailsPath = inf.RootDirectory + "live\\Tails.module";
                if (!inf.IsReady && inf.DriveType == DriveType.Removable || File.Exists(checkTailsPath) && inf.DriveType == DriveType.Removable)
                    btfDriveIsUp = true;
            }
        }
        private IntPtr UsbNotificationHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == UsbDetector.UsbDevicechange)
            {
                switch ((int)wparam)
                {
                    case UsbDetector.UsbDeviceRemoved:
                        CheckRemoveble();
                        if (btfDriveIsUp)
                            LogSuccessfull();
                        else
                            LogFault();
                        break;
                    case UsbDetector.NewUsbDeviceConnected:
                        CheckRemoveble();
                        if (btfDriveIsUp)
                            LogSuccessfull();
                        else
                            LogFault();
                        break;
                }
            }

            handled = false;
            return IntPtr.Zero;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(Process.GetCurrentProcess().MainWindowHandle);
            //IntPtr windowHandle = hwndSource.Handle;
            hwndSource?.AddHook(UsbNotificationHandler);
        }

        private static void RestartNow()
        {
            Process.Start("ShutDown", "/r /o /f /t 00");
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartNow();
        }

        private bool _clicked;
        private Point _lmAbs;

        private void SecondMainWindow_OnMouseMove(object sender, MouseEventArgs e)
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

        private void SecondMainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clicked = true;
            _lmAbs = e.GetPosition(this);
            _lmAbs.Y = Convert.ToInt16(Top) + _lmAbs.Y;
            _lmAbs.X = Convert.ToInt16(Left) + _lmAbs.X;
        }

        private void SecondMainWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _clicked = false;
        }
    }
}
