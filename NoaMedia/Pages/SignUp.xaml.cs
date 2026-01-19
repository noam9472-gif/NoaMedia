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
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Page
    {
        public SignUp()
        {
            InitializeComponent();
        }

        // כפתור הרשמה - אחרי הצלחה עובר לדף הבית
        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewUserTextBox.Text) || string.IsNullOrEmpty(NewPasswordBox.Password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // הודעת הצלחה
            MessageBox.Show("Account created successfully!");

            // --- המעבר לדף הבית ---
            Home homePage = new Home();
            if (this.NavigationService != null)
            {
                this.NavigationService.Navigate(homePage);
            }
        }

        // כפתור חזרה למסך הכניסה
        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            LogIn loginPage = new LogIn();
            this.NavigationService.Navigate(loginPage);
        }
    }
}