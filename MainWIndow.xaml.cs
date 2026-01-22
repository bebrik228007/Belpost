using System.Windows;

namespace Belpost.Auth
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox.Password;

            MessageBox.Show("логин " + login, "пароль " + password);

            using (var db = new AppDb()) //!!! нет хеширования
            {
                var user = db.Users.FirstOrDefault(u => u.Username == login);

                if (user == null)
                {
                    MessageBox.Show("Такого пользователя не существует!");
                }

                if (user?.PasswordHash == password && user != null)
                {
                    MessageBox.Show($"Добро пожаловать, {user.Role}!");
                }
                else
                {
                    MessageBox.Show("Неверный пароль!");
                }
            }
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Collapsed;
            VisibleTextBox.Text = PasswordBox.Password;
            VisibleTextBox.Visibility = Visibility.Visible;

        }

        private void CheckBox1_UnChecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;
            VisibleTextBox.Visibility = Visibility.Hidden;
        }
    }
}
