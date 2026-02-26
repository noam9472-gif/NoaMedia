using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NoaMedia.Pages
{
    public partial class AdminPage : Page
    {
        // יצירת ממשק ה-API
        InterfaceAPI api = new InterfaceAPI();

        public AdminPage()
        {
            InitializeComponent();
            LoadAllData(); // קריאה לפונקציה שטוענת את הנתונים
        }

        private async void LoadAllData()
        {
            try
            {
                // 1. טעינת משתמשים
                UserList uList = await api.GetAllUsers();
                if (uList != null)
                {
                    dgAllUsers.ItemsSource = uList;
                }

                // 2. טעינת משתמשי פרימיום
                UserPremiumList pList = await api.GetAllUserPremiums();
                if (pList != null)
                {
                    dgPremiumUsers.ItemsSource = pList;
                }

                // 3. טעינת סרטים
                VideoList mList = await api.GetAllVideos(); // וודא שיש לך פונקציה כזו ב-API
                if (mList != null)
                {
                    dgMovies.ItemsSource = mList;
                }

                // 4. טעינת תגובות (ביקורות)
                // כאן ייתכן שתצטרך פונקציה שמביאה את כל התגובות במערכת
                // CommentList cList = await api.GetAllComments(); 
                // dgComments.ItemsSource = cList;

            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת נתוני ניהול: " + ex.Message);
            }
        }

        // אירוע ללחיצה על הוספת סרט
        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            // כאן תוכל לנווט לדף הוספת סרט חדש שיצרת בעבר
            // this.NavigationService.Navigate(new AddMoviePage());
        }
    }
}