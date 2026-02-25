using ApiInterface;
using Model;
using System.Windows;
using System.Windows.Controls;

namespace NoaMedia.Pages
{
    public partial class PremiumSalesPage : Page
    {
        private readonly InterfaceAPI api = new InterfaceAPI();

        public PremiumSalesPage()
        {
            InitializeComponent();
        }

        private async void ConfirmPremium_Click(object sender, RoutedEventArgs e)
        {
            // 1. השגת המשתמש המחובר מה-App (כדי לקבל את ה-ID שלו)
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null) return;

            try
            {
                // 2. יצירת אובייקט פרימיום חדש (או פשוט שליחת ה-ID ל-Insert)
                // הערה: לפי ה-InterfaceAPI שלך, הפונקציה מצפה לאובייקט UserPremium
                UserPremium premiumEntry = new UserPremium
                {
                    Id = myApp.LoggedInUser.Id
                };

                int result = await api.InsertUserPremium(premiumEntry);

                if (result == 1)
                {
                    MessageBox.Show("Welcome to the VIP club! 👑");
                    // 3. חזרה לדף הבית - הפעם כמשתמש פרימיום (true)
                    this.NavigationService.Navigate(new Home(true));
                }
                else
                {
                    MessageBox.Show("Failed to upgrade. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}