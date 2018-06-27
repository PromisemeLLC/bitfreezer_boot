using System;
using System.Windows;
using System.Windows.Input;

namespace TailsBoot
{
    /// <summary>
    /// Interaction logic for BtfUpdateAcceptWindow.xaml
    /// </summary>
    public partial class BtfUpdateAcceptWindow
    {
        public BtfUpdateAcceptWindow(string applicationName, BtfUpdateXml updateInfo)
        {
            InitializeComponent();

            Title = applicationName + " - Update Avilable";
            lblNewVersion.Text = $"New Version: {updateInfo.Version}";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool _clicked;
        private Point _lmAbs;

        private void BtfUpdateAcceptWindow_OnMouseMove(object sender, MouseEventArgs e)
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

        private void BtfUpdateAcceptWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clicked = true;
            _lmAbs = e.GetPosition(this);
            _lmAbs.Y = Convert.ToInt16(Top) + _lmAbs.Y;
            _lmAbs.X = Convert.ToInt16(Left) + _lmAbs.X;
        }

        private void BtfUpdateAcceptWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _clicked = false;
        }
    }
}
