using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static Belpost.Auth.Hash;

namespace Belpost.Auth
{
    /// <summary>
    /// Логика взаимодействия для Password_Change.xaml
    /// </summary>
    public partial class Password_Change : Window
    {
        private readonly string _username;
        private DispatcherTimer timer = null;
        private int x = 0;

        public Password_Change(string username)
        {
            InitializeComponent();
            _username = username;
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldpassword = OldPasswordBox.Password;
            string newpassword = NewPasswordBox.Password;
            string confirmpassword = ConfirmPasswordBox.Password;

            
            if (!PasswordPolicy.Validate(newpassword))
            {
                MessageBox.Show("Пароль не соответствует требованиям.");
                return;
            }

            using (var db = new AppDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == _username);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден.");
                    return;
                }

         
                if (!PasswordHasher.Verify(oldpassword, user.PasswordHash, user.PasswordSalt))
                {
                    MessageBox.Show("Старый пароль введён неверно.");
                    return;
                }

           
                if (newpassword != confirmpassword)
                {
                    MessageBox.Show("Пароли не совпадают!");
                    return;
                }

                if (newpassword == oldpassword)
                {
                    MessageBox.Show("Новый пароль не должен совпадать со старым!");
                    return;
                }

                var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                DateTime moscowNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);

                var (hash, salt) = PasswordHasher.Hash(newpassword);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                user.PasswordChangedAt = moscowNow;

                db.SaveChanges();

                MessageBox.Show("Пароль успешно изменён!");
                this.Close();

                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            var mainWindow = new MainWindow();
            mainWindow.Show();
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

                var mainWindow = new MainWindow();
                mainWindow.Show();

                var register = new Register();
                register.Close();

            }
            else
            {
                x++;
            }

        }


    }
}
