using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using static Belpost.Auth.Hash;

namespace Belpost.Auth
{
    public partial class Register : Window
    {
        private DispatcherTimer timer = null;
        private int x = 0;

        public Register()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string role = (RoleBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string password = PasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (password != confirm)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            if (!PasswordPolicy.Validate(password))
            {
                MessageBox.Show("Пароль не соответствует требованиям");
                return;
            }

            var (hash, salt) = PasswordHasher.Hash(password);

            var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime moscowNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);

            var user = new User
            {
                Username = username,
                Role = role,
                PasswordHash = hash,
                PasswordSalt = salt,
                PasswordChangedAt = moscowNow,
                FailedAttempts = 0,
                IsLocked = false,
            };

            try
            {
                App.Db.Users.Add(user);
                App.Db.SaveChanges();
            }
            catch (NullReferenceException exception)
            {
                Console.WriteLine("Ошибка " + exception.Message);
            }

            MessageBox.Show("Пользователь успешно зарегистрирован!");

            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        
        private void Minute_Timer(object sender, RoutedEventArgs e)
        {
            timerStart();
        }

        private void timerStart()
        {
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Start();
        }

        private void UserActivity()
        {
            x = 0;
        }

        private void Register_MouseMove(object sender, MouseEventArgs e) => UserActivity();
        private void Register_MouseDown(object sender, MouseButtonEventArgs e) => UserActivity();
        private void Register_KeyDown(object sender, KeyEventArgs e) => UserActivity();



        private void timerTick(object sender, EventArgs e)
        {
            if (x >= 60)
            {
                this.Close();
            }
            else
            {
                x++;
            }

        }
    }
}