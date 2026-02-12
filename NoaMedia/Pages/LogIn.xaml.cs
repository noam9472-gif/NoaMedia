using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ApiInterface;
using Model;

namespace NoaMedia.Pages
{
    public partial class LogIn : Page
    {
        public static User currentUser=null; // משתנה סטטי שיכיל את המשתמש הנוכחי אחרי התחברות מוצלחת
        InterfaceAPI api = new InterfaceAPI();

        public LogIn()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            try
            {

                // זה ימנע מהמסך לקפוא בזמן שהנתונים נטענים
                UserList uList = await api.GetAllUsers();

                // מציאת המשתמש ברשימה
                currentUser = uList.Find(u => u.UserName == username && u.Pass == password);

                if (currentUser == null)
                {
                    MessageBox.Show("Invalid username or password.");
                    return;
                }
                    else
                    {
                        //שמירת המשתמש כדי שיופיע במסך הפרופיל
                        var myApp = Application.Current as App;
                        if (myApp != null)
                        {
                            myApp.LoggedInUser = currentUser;
                        }

                        // מעבר לדף הבית
                        NavigationService.Navigate(new Home());
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            SignUp registerPage = new SignUp();
            this.NavigationService.Navigate(registerPage);
        }
    }
}