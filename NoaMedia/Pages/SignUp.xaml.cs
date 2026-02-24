using Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ApiInterface; 

namespace NoaMedia.Pages
{
    public partial class SignUp : Page
    {
        
        InterfaceAPI api = new InterfaceAPI();

        public SignUp()
        {
            InitializeComponent();
        }

        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewUserTextBox.Text) || string.IsNullOrEmpty(NewPasswordBox.Password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            try
            {
                User newUser = new User
                {
                    UserName = NewUserTextBox.Text,
                    Pass = NewPasswordBox.Password 
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
            this.NavigationService.GoBack();
        }
    }
}