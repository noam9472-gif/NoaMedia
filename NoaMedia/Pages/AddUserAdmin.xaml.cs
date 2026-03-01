using ApiInterface;
using Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NoaMedia.Pages
{
    public partial class AddUserAdmin : Page
    {
        InterfaceAPI api = new InterfaceAPI();

        public AddUserAdmin()
        {
            InitializeComponent();
        }

        private async void SaveUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // בדיקות תקינות
                if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtPassword.Password))
                {
                    MessageBox.Show("Username and Password are required!");
                    return;
                }

                // יצירת אובייקט משתמש
                User newUser = new User
                {
                    Name = txtUserName.Text,
                    UserName = txtName.Text,
                    Mail = txtEmail.Text,
                    Pass = txtPassword.Password, 
                    DateOfBirth = dpBirthDate.SelectedDate ?? DateTime.Now.AddYears(-18),
                    IsAdmin = chkIsAdmin.IsChecked ?? false
                };

                int result = await api.InsertUser(newUser);

                if (result == 1)
                {
                    MessageBox.Show("User created successfully!");
                    this.NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Failed to create user. Username might already exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}