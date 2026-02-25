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
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("נא להזין שם משתמש וסיסמה.");
                return;
            }

            try
            {
                // 1. קבלת המשתמשים הכלליים
                UserList uList = await api.GetAllUsers();

                currentUser = uList?.FirstOrDefault(u =>
                    u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Pass == password);

                if (currentUser == null)
                {
                    MessageBox.Show("שם משתמש או סיסמה שגויים.");
                    return;
                }

                // 2. הבדיקה שביקשת: האם ה-ID שלו קיים בטבלת הפרימיום?
                // אנחנו משתמשים בפונקציה הקיימת שלך GetAllUserPremiums
                UserPremiumList pList = await api.GetAllUserPremiums();
                bool isPremium = pList != null && pList.Any(p => p.Id == currentUser.Id);

                // 3. שמירת המשתמש באפליקציה
                var myApp = Application.Current as App;
                if (myApp != null)
                {
                    myApp.LoggedInUser = currentUser;
                }

                // 4. ניתוב למסך המתאים
                if (isPremium)
                {
                    MessageBox.Show("שלום VIP! עובר למסך פרימיום...");
                    // כאן תעבור למסך הבית עם הצעות מורחבות
                    // בתוך ה-LoginButton_Click אחרי שגילית אם הוא פרימיום:
                    this.NavigationService.Navigate(new Home(isPremium));
                }
                else
                {
                    this.NavigationService.Navigate(new Home(false));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("תקלה בתקשורת: " + ex.Message);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }
    }
}