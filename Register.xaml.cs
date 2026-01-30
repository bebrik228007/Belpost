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
        private int inactivityCounter = 0;

        public Register()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
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
                IsLocked = false
            };

            try
            {
           
                App.Db.Users.Add(user);
                App.Db.SaveChanges();

                App.Db.PasswordHistory.Add(new PasswordHistory
                {
                    UserId = user.Id,
                    PasswordHash = hash,
                    ChangedAt = moscowNow
                });

                App.Db.SaveChanges();

                MessageBox.Show("Пользователь успешно зарегистрирован!");

                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

      
        private void Minute_Timer(object sender, RoutedEventArgs e)
        {
            StartInactivityTimer();
        }

        private void StartInactivityTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void ResetActivity()
        {
            inactivityCounter = 0;
        }

        private void Register_MouseMove(object sender, MouseEventArgs e) => ResetActivity();
        private void Register_MouseDown(object sender, MouseButtonEventArgs e) => ResetActivity();
        private void Register_KeyDown(object sender, KeyEventArgs e) => ResetActivity();

        private void TimerTick(object sender, EventArgs e)
        {
            if (inactivityCounter >= 60)
            {
                MessageBox.Show("Приложение закрыто из-за бездействия.");
                this.Close();
                new MainWindow().Show();
            }
            else
            {
                inactivityCounter++;
            }
        }
    }
}
