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
        private DispatcherTimer? timer = null;
        private int inactivityCounter = 0;

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

              
                if ((DateTime.Now - user.PasswordChangedAt).TotalDays < 5)
                {
                    MessageBox.Show("Пароль можно менять только через 5 дней после последней смены.");
                    return;
                }

            
                var last5 = db.PasswordHistory
                              .Where(p => p.UserId == user.Id)
                              .OrderByDescending(p => p.ChangedAt)
                              .Take(5)
                              .Select(p => p.PasswordHash)
                              .ToList();

                var (hash, salt) = PasswordHasher.Hash(newpassword);

                if (last5.Any(oldHash => oldHash == hash))
                {
                    MessageBox.Show("Новый пароль совпадает с одним из последних 5 паролей.");
                    return;
                }

             
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                user.PasswordChangedAt = DateTime.Now;

                db.PasswordHistory.Add(new PasswordHistory
                {
                    UserId = user.Id,
                    PasswordHash = hash,
                    ChangedAt = DateTime.Now
                });

                db.SaveChanges();

                MessageBox.Show("Пароль успешно изменён!");
                this.Close();

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

        private void Password_Change_MouseMove(object sender, MouseEventArgs e) => ResetActivity();
        private void Password_Change_MouseDown(object sender, MouseButtonEventArgs e) => ResetActivity();
        private void Password_Change_KeyDown(object sender, KeyEventArgs e) => ResetActivity();

        private void TimerTick(object sender, EventArgs e)
        {
            if (inactivityCounter >= 60)
            {
                timer.Stop();
                MessageBox.Show("Смена пароля закрыта из-за бездействия.");
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
