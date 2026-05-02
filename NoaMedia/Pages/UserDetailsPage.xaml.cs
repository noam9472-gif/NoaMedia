using ApiInterface;
using Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoaMedia.Pages
{
    public partial class UserDetailsPage : Page
    {
        private InterfaceAPI api = new InterfaceAPI();
        private User selectedUser;

        public UserDetailsPage(User user)
        {
            InitializeComponent();
            selectedUser = user;
            UserHeader.Text = $"Activity Report: {user.UserName}";
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                // טעינת כל הנתונים מהAPI
                var allVideos = await api.GetAllVideos();
                var allLikes = await api.GetAllLikes();
                var allReviews = await api.GetAllVideoReviews();

                // סינון בטוח, בודקים שהכל לא null
                if (allVideos != null)
                    lstUploadedVideos.ItemsSource = allVideos.Where(v => v.WhoUploadedTheVideo != null && v.WhoUploadedTheVideo.Id == selectedUser.Id).ToList();
                // הוספת סינון בטוח ללייקים
                if (allLikes != null)
                    lstLikedVideos.ItemsSource = allLikes.Where(l => l.UserId != null && l.UserId.Id == selectedUser.Id).ToList();
                // הוספת סינון בטוח לביקורות
                if (allReviews != null)
                {
                    lstReviews.ItemsSource = allReviews.Where(r => r.WhoUpdatedTheReview != null && r.WhoUpdatedTheReview.Id == selectedUser.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user details: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        private void VideoList_MouseDoubleClick(object sender, MouseButtonEventArgs e) //  טיפול בלחיצה כפולה על פריט ברשימת הסרטים
        {
            var listBox = sender as ListBox;
            if (listBox?.SelectedItem == null) return;

            Video videoToOpen = null;

            if (listBox.SelectedItem is Video v)
            {
                videoToOpen = v;
            }
            else if (listBox.SelectedItem is MyLikes like)
            {
                videoToOpen = like.VideoId;
            }
            // הוספת תמיכה למעבר לסרט מתוך רשימת הביקורות
            else if (listBox.SelectedItem is VideoReview review)
            {
                videoToOpen = review.WhichVideoDidTheUserReview;
            }

            if (videoToOpen != null)
            {
                this.NavigationService.Navigate(new VideoDetailsPage(videoToOpen));
            }
        }
    }
}