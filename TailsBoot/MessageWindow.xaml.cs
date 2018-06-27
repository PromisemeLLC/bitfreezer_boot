using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace TailsBoot
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow
    {
        public MessageWindow(string message)
        {
            InitializeComponent();

            txtMessage.Text = message;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool _clicked;
        private Point _lmAbs;

        private void MessageWindow_OnMouseMove(object sender, MouseEventArgs e)
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

        private void MessageWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clicked = true;
            _lmAbs = e.GetPosition(this);
            _lmAbs.Y = Convert.ToInt16(Top) + _lmAbs.Y;
            _lmAbs.X = Convert.ToInt16(Left) + _lmAbs.X;
        }

        private void MessageWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _clicked = false;
        }
    }
}
