using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NoaMedia.Pages; // וודא שזה תואם לשם הפרויקט שלך

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

            // בדיקה בסיסית שהשדות לא ריקים
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("נא להזין שם משתמש וסיסמה.");
                return;
            }

            try
            {
                // 1. שליפת כל המשתמשים מהשרת
                UserList uList = await api.GetAllUsers();

                // 2. חיפוש המשתמש הספציפי ברשימה
                currentUser = uList?.FirstOrDefault(u =>
    u.Name != null && u.Name.Trim().Equals(username, StringComparison.OrdinalIgnoreCase) &&
    u.Pass == password);

                // אם המשתמש לא נמצא - עוצרים כאן
                if (currentUser == null)
                {
                    MessageBox.Show("שם משתמש או סיסמה שגויים.");
                    return;
                }

                // 3. בדיקה אם הוא פרימיום (לפי טבלת הפרימיום)
                UserPremiumList pList = await api.GetAllUserPremiums();
                bool isPremium = pList != null && pList.Any(p => p.Id == currentUser.Id);

                // --- הוספת סנכרון פה ---
                // אם המשתמש נמצא בטבלת פרימיום אבל השדה IsPremium שלו הוא false, נעדכן אותו
                if (isPremium && !currentUser.IsPremium)
                {
                    currentUser.IsPremium = true;
                    await api.UpdateUser(currentUser);
                }
                // -----------------------

                // 4. שמירת המשתמש ב-App לשימוש גלובלי
                var myApp = Application.Current as App;
                // ... המשך הקוד שלך
                if (myApp != null)
                {
                    myApp.LoggedInUser = currentUser;
                }

                // 5. ניתוב (Routing) לפי סוג המשתמש
                if (this.NavigationService != null)
                {
                    if (currentUser.IsAdmin)
                    {
                        // מקרה א': המשתמש הוא מנהל - נשלח אותו לדף בחירת מסלול
                        MessageBox.Show("שלום מנהל! מעבר לדף אפשרויות...");
                        this.NavigationService?.Navigate(new TransitionOptionForManager(isPremium));
                    }
                    else if (isPremium)
                    {
                        // מקרה ב': המשתמש הוא פרימיום (ולא מנהל) - נשלח אותו למסך הבית VIP
                        MessageBox.Show("ברוך הבא VIP! צפייה מהנה.");
                        this.NavigationService.Navigate(new Home(true));
                    }
                    else
                    {
                        // מקרה ג': משתמש רגיל - נשלח אותו למסך הבית הרגיל
                        this.NavigationService.Navigate(new Home(false));
                    }
                }
            }
            catch (Exception ex)
            {
                // אם משהו קורס בדרך (בעיית תקשורת או שדה חסר ב-DB) - תקבל הודעה
                MessageBox.Show("שגיאה בתהליך ההתחברות: " + ex.Message);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }
    }
}