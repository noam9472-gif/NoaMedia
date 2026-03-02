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

                // 1. יצירת אובייקט משתמש - שים לב ש-IsAdmin תמיד false
                User newUser = new User
                {
                    Name = txtName.Text,         // תיקנתי כאן לפי שמות השדות המקובלים
                    UserName = txtUserName.Text,
                    Mail = txtEmail.Text,
                    Pass = txtPassword.Password,
                    DateOfBirth = dpBirthDate.SelectedDate ?? DateTime.Now.AddYears(-18),
                    IsAdmin = false // רק אתה המנהל, המשתמשים החדשים לעולם לא יהיו מנהלים
                };

                // 2. הכנסת המשתמש למסד הנתונים
                int result = await api.InsertUser(newUser);

                if (result == 1)
                {
                    // 3. אם סימנו "Grant Premium", נוסיף אותו לטבלת הפרימיום
                    if (chkIsAdmin.IsChecked == true)
                    {
                        // קודם נשלוף את המשתמש שזה עתה יצרנו כדי לקבל את ה-ID שלו
                        UserList uList = await api.GetAllUsers();
                        User createdUser = uList.FirstOrDefault(u => u.UserName == newUser.UserName);

                        if (createdUser != null)
                        {
                            // יצירת אובייקט פרימיום וקישורו ל-ID של המשתמש
                            UserPremium premiumEntry = new UserPremium { Id = createdUser.Id };
                            await api.InsertUserPremium(premiumEntry);
                        }
                    }

                    MessageBox.Show("User created successfully (with Premium if selected)!");
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