using ApiInterface;
using Model;
using System;
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
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null) return;

            int userId = myApp.LoggedInUser.Id;

            try
            {
                int commentsCount = await api.GetCommentsCountByUser(userId);
                int likesCount = await api.GetLikesCountByUser(userId);

                if (commentsCount >= 5 && likesCount >= 5)
                {
                    int result = await api.UpgradeUserToPremium(userId);

                    if (result >= 1)
                    {
                        myApp.LoggedInUser.IsPremium = true;
                        MessageBox.Show("Success! Your account is now Premium. 👑");

                        // תיקון: שולחים את המשתמש עצמו במקום בוליאני
                        this.NavigationService.Navigate(new Home(myApp.LoggedInUser));
                    }
                    else
                    {
                        MessageBox.Show("Update failed. Please try again.");
                    }
                }
                else
                {
                    MessageBox.Show("You need 5 likes and 5 comments to upgrade!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}