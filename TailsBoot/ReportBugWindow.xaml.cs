using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;

namespace TailsBoot
{
    /// <summary>
    /// Interaction logic for ReportBugWindow.xaml
    /// </summary>
    public partial class ReportBugWindow
    {
        public ReportBugWindow(string message)
        {
            InitializeComponent();
            _messageBody = message;
        }

        private readonly string _messageBody;
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var rx = new Regex(@"^\w+$");
            if (rx.IsMatch(txtUsername.Text))
            {
                var emR = new Regex(@"^[_a-z0-9-]+(.[a-z0-9-]+)@[a-z0-9-]+(.[a-z0-9-]+)*(.[a-z]{2,4})$");
                if (emR.IsMatch(txtEmail.Text))
                {
                    await SendReport();
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    var mbw = new MessageWindow("Email adress is not valid");
                    mbw.ShowDialog();
                }
            }
            else
            {
                var mbw = new MessageWindow("Please, put valid username");
                mbw.ShowDialog();
            }
        }

        private async Task SendReport()
        {
             var token = "your token here";
             ChatId developerChatId = 0;
            try
            {
                var botClient = new Telegram.Bot.TelegramBotClient(token);

                await botClient.SendTextMessageAsync(developerChatId, $"<b>Username: </b>{txtUsername.Text}\n<b>Email: </b>{txtEmail.Text}\n<b>Message: </b><i>{_messageBody}</i>", ParseMode.Html);
                

            }
            catch
            {
                // ignored
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtEmail.Text != "Email") return;
            txtEmail.Foreground = Brushes.Black;
            txtEmail.Text = "";
        }

        private void TextBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text != "Username") return;
            txtUsername.Foreground = Brushes.Black;
            txtUsername.Text = "";
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtEmail.Text != "") return;
            var brush = new SolidColorBrush(Color.FromArgb(255, 108, 108, 108));
            txtEmail.Foreground = brush;
            txtEmail.Text = "Email";
            
        }

        private void TextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtUsername.Text != "") return;
                var brush = new SolidColorBrush(Color.FromArgb(255, 108, 108, 108));
                txtUsername.Foreground = brush;
                txtUsername.Text = "Username";
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private bool _clicked;
        private Point _lmAbs;

        private void ReportBugWindow_OnMouseMove(object sender, MouseEventArgs e)
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

        private void ReportBugWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clicked = true;
            _lmAbs = e.GetPosition(this);
            _lmAbs.Y = Convert.ToInt16(Top) + _lmAbs.Y;
            _lmAbs.X = Convert.ToInt16(Left) + _lmAbs.X;
        }

        private void ReportBugWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _clicked = false;
        }
    }
}
