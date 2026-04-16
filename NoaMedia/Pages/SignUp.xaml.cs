using Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ApiInterface;
using System.Text.RegularExpressions; // חשוב לצורך בדיקת המייל

namespace NoaMedia.Pages
{
    public partial class SignUp : Page
    {
        InterfaceAPI api = new InterfaceAPI();

        public SignUp()
        {
            InitializeComponent();
        }

        // פונקציה לבדיקת תקינות אימייל לפי התבנית שביקשת
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            // בדיקה שיש @, אחריו תווים, אחריו נקודה ואז שוב תווים
            string pattern = @"^[a-zA-Z0-9]+@[a-zA-Z0-9]+\.[a-zA-Z0-9]+$";
            return Regex.IsMatch(email, pattern);
        }

        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            // בדיקה שכל השדות מלאים
            if (string.IsNullOrEmpty(NewUserTextBox.Text) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(FullNameTextBox.Text) ||
                string.IsNullOrEmpty(EmailTextBox.Text) ||
                BirthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // בדיקת תקינות מייל
            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Please enter a valid email address (e.g., user@domain.com).");
                return;
            }

            try
            {
                // יצירת משתמש חדש עם כל הנתונים
                User newUser = new User
                {
                    Name = NewUserTextBox.Text,
                    Pass = NewPasswordBox.Password,
                    UserName = FullNameTextBox.Text,
                    Mail = EmailTextBox.Text,
                    DateOfBirth = BirthDatePicker.SelectedDate.Value,
                    IsAdmin = false,    // ברירת מחדל:לא מנהל
                    IsPremium = false   // ברירת מחדל:לא פרימיום
                };

                await api.InsertUser(newUser);

                MessageBox.Show("Account created successfully! You can now log in.");
                this.NavigationService.Navigate(new Log_in());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration failed: " + ex.Message);
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
            else
                this.NavigationService.Navigate(new Log_in());
        }
    }
}