using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using static Belpost.Auth.Hash;

namespace Belpost.Auth
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer lockTimer;
        private int lockSecondsRemaining;

        public MainWindow()
        {
            InitializeComponent();
            TryResumeLockCountdownIfNeeded();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox.Visibility == Visibility.Visible
                ? PasswordBox.Password
                : VisibleTextBox.Text;

            using (var db = new AppDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == login);

                if (user == null)
                {
                    MessageBox.Show("Такого пользователя не существует!");
                    return;
                }

                var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                DateTime nowMoscow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);

                if ((nowMoscow - user.PasswordChangedAt).TotalDays > 5)
                {
                    MessageBox.Show("С момента смены пароля прошло 5 дней! Нужно сменить пароль.");
                    return;
                }


                if (user.IsLocked && user.LockUntil.HasValue && DateTime.UtcNow < user.LockUntil.Value)
                {
                    var secondsLeft = (int)(user.LockUntil.Value - DateTime.UtcNow).TotalSeconds;
                    MessageBox.Show($"Аккаунт заблокирован. Подождите {secondsLeft} секунд.");
       
                    StartLockTimer(user.Username);
                    return;
                }

      
                if (PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
                {
                    MessageBox.Show($"Добро пожаловать, {user.Role}!");
                    ClearInputs();

                    user.FailedAttempts = 0;
                    user.IsLocked = false;
                    user.LockUntil = null;
                    db.SaveChanges();
                }
                else
                {
                    user.FailedAttempts++;
                    if (user.FailedAttempts >= 3)
                    {
                        user.IsLocked = true;
                        user.FailedAttempts = 0;
                        user.LockUntil = DateTime.UtcNow.AddMinutes(1);
                        db.SaveChanges();

                        MessageBox.Show("Вы ввели пароль неправильно 3 раза! Аккаунт заблокирован на 1 минуту.");
                        StartLockTimer(user.Username);
                    }
                    else
                    {
                        db.SaveChanges();
                        MessageBox.Show("Неверный пароль!");
                    }
                }
            }
        }

        private void StartLockTimer(string username)
        {

            using (var db = new AppDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null || !user.LockUntil.HasValue) return;

                var remaining = (int)(user.LockUntil.Value - DateTime.UtcNow).TotalSeconds;
                lockSecondsRemaining = remaining > 0 ? remaining : 0;
            }


            if (lockTimer != null && lockTimer.IsEnabled) return;

            TimeBox.Text = "* " + lockSecondsRemaining.ToString();

            lockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            lockTimer.Tick += (s, e) =>
            {
                lockSecondsRemaining--;
                if (lockSecondsRemaining < 0) lockSecondsRemaining = 0;
                TimeBox.Text = "* " + lockSecondsRemaining.ToString();

                if (lockSecondsRemaining <= 0)
                {
                    lockTimer.Stop();
                    lockTimer = null;

                    using (var db = new AppDb())
                    {
                        var user = db.Users.FirstOrDefault(u => u.Username == username);
                        if (user != null)
                        {

                            user.IsLocked = false;
                            user.FailedAttempts = 0;
                            user.LockUntil = null;
                            db.SaveChanges();
                        }
                    }

                    MessageBox.Show("Аккаунт разблокирован!");
                }
            };
            lockTimer.Start();
        }


        private void TryResumeLockCountdownIfNeeded()
        {
            using (var db = new AppDb())
            {
                string currentUsername = LoginBox.Text; 

                if (string.IsNullOrWhiteSpace(currentUsername))
                    return;

                var user = db.Users.FirstOrDefault(u => u.Username == currentUsername);

                if (user != null && user.IsLocked && user.LockUntil.HasValue && DateTime.UtcNow < user.LockUntil.Value)
                {
                   
                    int secondsLeft = (int)(user.LockUntil.Value - DateTime.UtcNow).TotalSeconds;

                    MessageBox.Show($"Аккаунт заблокирован. Подождите {secondsLeft} секунд.");
                    StartLockTimer(user.Username);
                }
            }
        }


        private void ClearInputs()
        {
            LoginBox.Text = "";
            PasswordBox.Password = "";
            VisibleTextBox.Text = "";
            TimeBox.Text = "";
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            VisibleTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            VisibleTextBox.Visibility = Visibility.Visible;
        }

        private void CheckBox1_UnChecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = VisibleTextBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            VisibleTextBox.Visibility = Visibility.Collapsed;
        }

        private void V_Budushem_Dobavlu(object sender, RoutedEventArgs e)
        {
          
        }

        private void Register_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var registerWindow = new Register();
            registerWindow.Show();
            this.Hide();
        }
    }
}
