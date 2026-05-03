using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NoaMedia.Pages;

namespace NoaMedia.Pages
{
    public partial class Log_in : Page
    {
        public static User currentUser = null;
        InterfaceAPI api = new InterfaceAPI();

        public Log_in()
        {
            InitializeComponent();
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e) // כפתור להראות/להסתיר סיסמה
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                ShowPasswordButton.Content = "🙈";
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                ShowPasswordButton.Content = "👁";
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e) // כפתור להתחברות
        {
            string username = UsernameTextBox.Text.Trim();
            string password = (PasswordBox.Visibility == Visibility.Visible)
                ? PasswordBox.Password
                : PasswordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) // בדיקה אם השדות ריקים
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            try
            {
                UserList uList = await api.GetAllUsers(); // שליפת כל המשתמשים מהשרת

                currentUser = uList?.FirstOrDefault(u =>
                    u.Name != null && u.Name.Trim().Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Pass == password); // חיפוש משתמש תואם בשם וסיסמה, שם משתמש לא רגיש לרווחים או לאותיות גדולות/קטנות

                if (currentUser == null)
                {
                    MessageBox.Show("User Name or Password is incorrect.");
                    return;
                }

                UserPremiumList pList = await api.GetAllUserPremiums();
                bool isPremium = (pList != null && pList.Any(p => p.Id == currentUser.Id)) || currentUser.IsPremium;

                if (isPremium && !currentUser.IsPremium)
                {
                    currentUser.IsPremium = true;
                    await api.UpdateUser(currentUser);
                }

                // עדכון האפליקציה לגבי המשתמש המחובר
                var myApp = Application.Current as App;
                if (myApp != null)
                {
                    myApp.LoggedInUser = currentUser;
                }

                if (this.NavigationService != null)
                {
                    if (currentUser.IsAdmin)
                    {
                        //  שליחה לדף המעבר במקום לדף הבית
                        this.NavigationService.Navigate(new TransitionOptionForManager(isPremium));
                    }
                    else
                    {
                        if (isPremium)  // שליחה לדף הבית עם פרמיום אם המשתמש פרימיום, אחרת לדף הבית רגיל
                            this.NavigationService.Navigate(new Home(currentUser));
                        else { this.NavigationService.Navigate(new Home(currentUser));  }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during login process: " + ex.Message);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }
    }
}