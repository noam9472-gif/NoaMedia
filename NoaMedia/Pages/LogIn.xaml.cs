using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NoaMedia.Pages
{
    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    public partial class LogIn : Page
    {
        public LogIn()
        {
            InitializeComponent();
        }

        // לחיצה על כפתור התחברות
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // בדיקת תקינות
            if (username == "admin" && password == "1234")
            {
                // יצירת דף הבית ומעבר אליו
                Home homePage = new Home();
                this.NavigationService.Navigate(homePage);
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
            this.NavigationService.Navigate(registerPage);
        }
    }
}