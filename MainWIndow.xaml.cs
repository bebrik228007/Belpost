using System.Windows;
using System.Linq;

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
            string password;

            if (PasswordBox.Visibility == Visibility.Visible)
                password = PasswordBox.Password;
            else
                password = VisibleTextBox.Text;


            using (var db = new AppDb()) 
            {
                var user = db.Users.FirstOrDefault(u => u.Username == login);

                if (user == null)
                {
                    MessageBox.Show("Такого пользователя не существует!");
                    return;
                }

                if (user.PasswordHash == password)
                {
                    MessageBox.Show($"Добро пожаловать, {user.Role}!");
                }
                else
                {
                    user.FailedAttempts += 1;
                    db.SaveChanges(); 
                    MessageBox.Show("Неверный пароль!");
                }
            }
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
    }
}
