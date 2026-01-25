using System;
using System.Linq;
using System.Windows;
using static Belpost.Auth.Hash;

namespace Belpost.Auth
{
    /// <summary>
    /// Логика взаимодействия для Password_Change.xaml
    /// </summary>
    public partial class Password_Change : Window
    {
        private readonly string _username;

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

                var (hash, salt) = PasswordHasher.Hash(newpassword);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;

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
    }
}
