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
                // יצירת משתמש חדש עם התכונות מהטופס
                User newUser = new User
                {
                    Name = txtUserName.Text,         
                    UserName = txtName.Text,
                    Mail = txtEmail.Text,
                    Pass = txtPassword.Password,
                    DateOfBirth = dpBirthDate.SelectedDate ?? DateTime.Now.AddYears(-18), // אם לא נבחר תאריך, נניח שהמשתמש בן 18
                    IsAdmin = false // תמיד ניצור משתמש רגיל, הפרימיום יתווסף בנפרד אם נבחר באופציה המתאימה
                };

                //  הכנסת המשתמש למסד הנתונים
                int result = await api.InsertUser(newUser);

                if (result == 1)
                {
                    // אם המנהל סימן "Grant Premium", נוסיף אותו לטבלת הפרימיום
                    if (chkIsAdmin.IsChecked == true)
                    {
                        UserList uList = await api.GetAllUsers();
                        // נניח שהשם משתמש הוא ייחודי, נחפש את המשתמש החדש לפי שם המשתמש שלו
                        User createdUser = uList.FirstOrDefault(u => u.UserName == newUser.UserName); 

                        if (createdUser != null)
                        {
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