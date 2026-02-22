using ApiInterface;
using Model;
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
using NoaMedia;

namespace NoaMedia.Pages
{
    /// <summary>
    /// Interaction logic for Log_in.xaml
    /// </summary>
    public partial class Log_in : Page
    {
        public static User currentUser = null;
        InterfaceAPI api = new InterfaceAPI();

        public Log_in()
        {
            InitializeComponent();
        }


        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim(); // Trim מנקה רווחים מיותרים
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("נא להזין שם משתמש וסיסמה.");
                return;
            }

            try
            {
                // קבלת הרשימה מהשרת
                UserList uList = await api.GetAllUsers();

                // שלב הבדיקה 
                if (uList == null || uList.Count == 0)
                {
                    MessageBox.Show("שגיאה: רשימת המשתמשים ריקה ([]). \nהשרת מחובר אבל לא מושך נתונים מהאקסס.");
                    return;
                }

                // חיפוש המשתמש 
                currentUser = uList.FirstOrDefault(u =>
                    u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Pass == password);

                if (currentUser == null)
                {
                    MessageBox.Show("שם משתמש או סיסמה שגויים.");
                    return;
                }

                var myApp = Application.Current as App;
                if (myApp != null)
                {
                    myApp.LoggedInUser = currentUser;
                }

                // מעבר לדף הבית
                this.NavigationService.Navigate(new Home());
            }
            catch (Exception ex)
            {
                MessageBox.Show("תקלה בתקשורת עם השרת: " + ex.Message);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }
    }
}