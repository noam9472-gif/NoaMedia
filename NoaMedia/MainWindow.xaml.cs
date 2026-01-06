using NoaMedia;
using System.Windows;

namespace NoamediaLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // לחיצה על כפתור התחברות
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (username == "admin" && password == "1234")
            {
                MessageBox.Show("Welcome to Noamedia!");
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        // לחיצה על כפתור מעבר להרשמה
        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            SignUp registerPage = new SignUp();
            registerPage.Show();
            this.Close(); // סוגר את חלון הכניסה הנוכחי
        }
    }
}