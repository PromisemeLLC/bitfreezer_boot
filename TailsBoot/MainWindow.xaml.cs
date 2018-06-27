using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace TailsBoot
{

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckRemoveble();
            if (btfDriveIsUp)
                LogSuccessfull();
            else
                LogFault();
        }

        private bool btfDriveIsUp;
        //private bool MetroStyle;
        private const string fileName = "bootID.dat";
        
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
                Color = (Color) ColorConverter.ConvertFromString("#FF7E0B0B"),
                Offset = 1
            };
            redLFB.GradientStops.Add(newRedGS);
            indicator.Fill = redLFB;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(Process.GetCurrentProcess().MainWindowHandle);
            //IntPtr windowHandle = hwndSource.Handle;
            hwndSource?.AddHook(UsbNotificationHandler);
            //UsbDetector.RegisterUsbDeviceNotification(windowHandle);
        }

        //private void CheckWinVersion()
        //{
        //    var win8Version = new Version(6, 2);

        //    if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
        //        Environment.OSVersion.Version >= win8Version)
        //    {
        //        MetroStyle = true;
        //    }
        //    else
        //    {
        //        MetroStyle = false;
        //    }
        //}

        private void CreateBootEntry()
        {
            var path = Path.GetPathRoot(Environment.SystemDirectory);
            if (!Directory.Exists(path + "BitFreezer"))
                Directory.CreateDirectory(path + "BitFreezer");
            const string mbrPath = "BitFreezer\\btf.mbr";
            if (File.Exists(path + mbrPath))
            {
                try
                {
                    File.Delete(path + mbrPath);
                }
                catch
                {
                    // ignored
                }
            } 
            GetDiskNum(path + mbrPath);
            if (File.Exists(path + mbrPath))
            {
                if (File.Exists(fileName))
                {
                    var id = DisplayId();
                    if (!string.IsNullOrEmpty(id))
                    {
                        SetDisplayOrder(id);
                    }
                    else
                    {
                        TemoMethodForDuplicate(path, mbrPath);
                    }
                }
                else
                {
                    TemoMethodForDuplicate(path, mbrPath);
                }
            }
            else
            {
                var mbw = new MessageWindow("Can't create Master Boot Record for BitFreezer");
                mbw.ShowDialog();
            }
            
        }

        private static void RestartNow()
        {
            Process.Start("ShutDown", "/r /t 5");
        }

        private void TemoMethodForDuplicate(string path, string mbrPath)
        {
            var argumentPath = path.Remove(path.Length - 1);
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "bcdedit.exe",
                    Arguments = "/create /d \"BitFreezer\" /application bootsector",
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
            var regex = new Regex("[{](.*)[}]");
            var v = regex.Match(output);
            var s = v.Groups[0].ToString();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(s))
            {
                
                WriteId(s);
                SetOptions(s, "device", $"partition={argumentPath}");
                SetOptions(s, "path", "\\" + mbrPath);
                //if (MetroStyle)
                //    SetOptions(s, "custom:250000c2", "1");
                SetDisplayOrder(s);
            }
            else
            {
                var mbw = new MessageWindow("Cannot create boot entry");
                mbw.ShowDialog();
            }
        }

        private static void SetOptions(string id, string option, string argument)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "bcdedit.exe",
                    Arguments = $"/set {id} {option} {argument}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    Verb = "runas"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static void SetDisplayOrder(string id)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "bcdedit.exe",
                    Arguments = $"/bootsequence {id}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                }
            };
            process.Start();
            process.WaitForExit();
            SetOptions("{bootmgr}", "displaybootmenu", "No");
            RestartNow();
        }

        private static void WriteId(string id)
        {
            var fs = new FileStream(fileName, FileMode.Create);
            var formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, id);
            }
            catch
            {
                // ignored
            }
            finally
            {
                fs.Close();
            }
        }

        private string DisplayId()
        {
            var bootId = "";

            var fs = new FileStream(fileName, FileMode.Open);
            try
            {
                var formatter = new BinaryFormatter();

                // Deserialize the hashtable from the file and 
                // assign the reference to the local variable.
                bootId = (string)formatter.Deserialize(fs);
            }
            catch
            {
                // ignored
            }
            finally
            {
                fs.Close();
            }
            return bootId;
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            CreateBootEntry();
            //CheckRemoveble();
        }

        private void GetDiskNum(string p)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "bin/BootGrabber.exe",
                    Arguments = "/list",
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
            process.WaitForExit();
            var s = output.Substring(output.IndexOf("\r\n-\r\n\tIndex: ", StringComparison.Ordinal));
            var sa = s.Split(new[] { "\r\n-" }, StringSplitOptions.None);
            var diskMagicNumber = -1;
            if (sa.Length > 0)
            {
                
                for (var i = 1; i < sa.Length; i++)
                {
                    //var bufferCmp1 = Regex.Matches(sa[i], "FileSystem: 40\r\n").Count;
                    //var bufferCmp2 = Regex.Matches(sa[i], "FileSystem: 175\r\n").Count;
                    if (!sa[i].Contains("PartitionCount: 2\r\n") || !sa[i].Contains("FileSystem: 40\r\n") ||
                        !sa[i].Contains("FileSystem: 175\r\n") || !sa[i].Contains("PrimaryCount: 0\r\n")) continue;
                    diskMagicNumber = i - 1;
                    break;
                }
                //File.WriteAllText("out.txt", s);
                if (diskMagicNumber != -1)
                {
                    GrabBootSector(diskMagicNumber, p);
                }
                
            }
            else
            {
                var mbw = new MessageWindow("Cannot find any Disks");
                mbw.ShowDialog();
            }
        }

        private void GrabBootSector(int diskMagicNumber, string saveFilePath)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "bin/BootGrabber.exe",
                    Arguments = $@"/grab /d {diskMagicNumber} /p 1 /file {saveFilePath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    Verb = "runas"
                }
            };
            process.Start();
            process.WaitForExit();
            var input = File.ReadAllBytes(saveFilePath);
            var fix = FixBootText(input);
            File.WriteAllBytes(saveFilePath, fix);
        }

        private byte[] FixBootText(byte[] input)
        {
            input[474] = 80;    //P
            input[475] = 114;   //r
            input[476] = 101;   //e
            input[477] = 115;   //s
            input[478] = 115;   //s
            input[479] = 32;    // 
            input[480] = 97;    //a
            input[481] = 110;   //n
            input[482] = 121;   //y
            input[483] = 32;    //
            input[484] = 107;   //k 
            input[485] = 101;   //e
            input[486] = 121;   //y
            return input;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private bool _clicked;
        private Point _lmAbs;

        private void PnMouseDown(object sender, MouseEventArgs e)
        {
            _clicked = true;
            _lmAbs = e.GetPosition(this);
            _lmAbs.Y = Convert.ToInt16(Top) + _lmAbs.Y;
            _lmAbs.X = Convert.ToInt16(Left) + _lmAbs.X;
        }

        private void PnMouseUp(object sender, MouseEventArgs e)
        {
            _clicked = false;
        }

        private void PnMouseMove(object sender, MouseEventArgs e)
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
    }
}
