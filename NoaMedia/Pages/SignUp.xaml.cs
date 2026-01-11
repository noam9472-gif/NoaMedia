using NoamediaLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        // כפתור הרשמה
        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewUserTextBox.Text) || string.IsNullOrEmpty(NewPasswordBox.Password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // שמירת משתמש
            MessageBox.Show("Account created successfully!");
        }

        // כפתור חזרה
        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            LogIn LogIn = new LogIn();
            this.NavigationService.Navigate(LogIn);
        }
    }
}
