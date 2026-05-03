using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NoaMedia.Pages
{
    public partial class ProfilePage : Page
    {
        
        private IInterfaceAPI api = new InterfaceAPI();
        private User currentUser;
        private List<Video> fullHistoryList = new List<Video>();

        public ProfilePage(string currentName) // קונסטרקטור שמקבל את שם המשתמש הנוכחי
        {
            InitializeComponent();
            currentUser = (Application.Current as App).LoggedInUser; // משיכת אובייקט המשתמש הנוכחי מהאפליקציה

            if (currentUser != null) // בדיקה אם המשתמש קיים
            {
                UserNameHeading.Text = currentUser.UserName; // הצגת שם המשתמש בכותרת
                ProfileInitialText.Text = currentUser.UserName.Substring(0, 1).ToUpper(); 

                if (currentUser.IsAdmin) // בדיקה אם המשתמש הוא מנהל
                    PremiumBadge.Visibility = Visibility.Visible;
            }
        }

        private async void LoadContent(string category) // טעינת תוכן לפי קטגוריה
        {
            MainDisplayPanel.Children.Clear();
            CommentsDisplayPanel.Children.Clear();
            PremiumLockPanel.Visibility = Visibility.Collapsed;
            ContentScrollViewer.Visibility = Visibility.Visible;
            ShowAllHistoryButton.Visibility = Visibility.Collapsed;
            MainDisplayPanel.Visibility = Visibility.Collapsed;
            CommentsDisplayPanel.Visibility = Visibility.Collapsed;

            try
            {
                switch (category) // בחירת הקטגוריה לטעינה בהתאם לבחירת המשתמש בתפריט הצדדי
                {
                    case "MyVideos": // טעינת הסרטים שהמשתמש העלה
                        SectionTitle.Text = "My Videos";
                        MainDisplayPanel.Visibility = Visibility.Visible;
                        var allVideos = (VideoList)await api.GetAllVideos();
                        var myVideos = allVideos.Where(v => v.WhoUploadedTheVideo?.Id == currentUser.Id).ToList();
                        FillVideoPanel(myVideos);
                        break;

                    case "Liked": // טעינת הסרטים שהמשתמש אהב
                        SectionTitle.Text = "Liked Videos";
                        MainDisplayPanel.Visibility = Visibility.Visible;
                        var allLikes = (MyLikesList)await api.GetAllLikes();
                        var myLikedVideos = allLikes
                            .Where(l => l.UserId?.Id == currentUser.Id)
                            .Select(l => l.VideoId)
                            .ToList();
                        FillVideoPanel(myLikedVideos);
                        break;

                    case "Watched": // טעינת היסטוריית הצפייה של המשתמש
                        SectionTitle.Text = "Watched History";
                        MainDisplayPanel.Visibility = Visibility.Visible;
                        var historyResponse = await api.GetAllMyHistory();
                        if (historyResponse == null) break;

                        List<MyHistory> allHistoryRecords = historyResponse.ToList();
                        // סינון, מיון וקיבוץ לפי וידאו כדי לקבל רשימה ייחודית של סרטים מההיסטוריה
                        fullHistoryList = allHistoryRecords 
                            .Where(h => h.UserId?.Id == currentUser.Id)
                            .OrderByDescending(h => h.Id)
                            .Select(h => h.VideoId)
                            .Where(v => v != null)
                            .GroupBy(v => v.Id)
                            .Select(g => g.First())
                            .ToList();

                        var partialHistory = fullHistoryList.Take(5).ToList();
                        FillVideoPanel(partialHistory);

                        if (fullHistoryList.Count > 5)
                            ShowAllHistoryButton.Visibility = Visibility.Visible;
                        break;

                    case "MyList": // טעינת רשימת הצפייה האישית של המשתמש
                        SectionTitle.Text = "My List";

                        // בדיקה: אם המשתמש הוא מנהל או שיש לו מנוי פרימיום - פתח את התוכן
                        if (currentUser.IsAdmin || currentUser.IsPremium)
                        {
                            // מוודא שהתוכן גלוי ומסך הנעילה מוסתר
                            ContentScrollViewer.Visibility = Visibility.Visible;
                            PremiumLockPanel.Visibility = Visibility.Collapsed;
                            // הצגת הפאנל עם הסרטים
                            MainDisplayPanel.Visibility = Visibility.Visible;

                            var allWatchList = (MyWatchListList)await api.GetAllMyWatchList();
                            var myPersonalList = allWatchList
                                .Where(w => w.UserId?.Id == currentUser.Id)
                                .Select(w => w.VideoId)
                                .Where(v => v != null)
                                .ToList();

                            FillVideoPanel(myPersonalList);
                        }
                        else
                        {
                            // אם הוא לא אדמין ולא פרימיום - הצג את מסך הנעילה
                            ContentScrollViewer.Visibility = Visibility.Collapsed;
                            PremiumLockPanel.Visibility = Visibility.Visible;
                        }
                        break; ;

                    case "Comments": // טעינת התגובות שהמשתמש כתב
                        SectionTitle.Text = "My Comments";
                        CommentsDisplayPanel.Visibility = Visibility.Visible;
                        var allComments = await api.GetAllVideoReviews();
                        // סינון התגובות כך שיוצגו רק אלו שהמשתמש הנוכחי כתב
                        var myComments = allComments.Where(c => c.WhoUpdatedTheReview.Id == currentUser.Id).ToList();

                        foreach (var comment in myComments)
                        {
                            // יצירת הקונטיינר הראשי לכל תגובה
                            Grid commentContainer = new Grid { Margin = new Thickness(0, 0, 0, 20) };
                            commentContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            commentContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                            StackPanel textStack = new StackPanel();

                            // שם הסרטון
                            TextBlock videoTitle = new TextBlock
                            {
                                Text = $"Video: {comment.WhichVideoDidTheUserReview?.VideoName ?? "Unknown Video"}",
                                Foreground = Brushes.Gray,
                                FontSize = 12,
                                Cursor = Cursors.Hand
                            };
                            videoTitle.MouseDown += (s, e) => this.NavigationService.Navigate(new MovieDetails(comment.WhichVideoDidTheUserReview));

                            // תוכן התגובה
                            TextBlock commentText = new TextBlock
                            {
                                Text = $"\"{comment.ReviewDescription}\"",
                                Foreground = Brushes.White,
                                FontSize = 16,
                                Margin = new Thickness(0, 5, 0, 0),
                                TextWrapping = TextWrapping.Wrap
                            };
                            // הוספת שם הסרטון ותוכן התגובה לסטקפאנל
                            textStack.Children.Add(videoTitle);
                            textStack.Children.Add(commentText);
                            textStack.Children.Add(new Separator { Background = Brushes.DimGray, Margin = new Thickness(0, 10, 0, 0) });

                            Grid.SetColumn(textStack, 0);
                            commentContainer.Children.Add(textStack);

                            // הוספת כפתור המחיקה
                            Button deleteBtn = new Button
                            {
                                Content = "🗑️", 
                                Background = Brushes.Transparent,
                                Foreground = Brushes.Gray,
                                BorderThickness = new Thickness(0),
                                FontSize = 18,
                                VerticalAlignment = VerticalAlignment.Top,
                                Cursor = Cursors.Hand,
                                Tag = comment // שומרים את האובייקט של התגובה בתוך הכפתור
                            };

                            // עיצוב למעבר עכבר
                            deleteBtn.MouseEnter += (s, e) => (s as Button).Foreground = Brushes.Red;
                            deleteBtn.MouseLeave += (s, e) => (s as Button).Foreground = Brushes.Gray;

                            // אירוע לחיצה על מחיקה
                            deleteBtn.Click += DeleteComment_Click;

                            Grid.SetColumn(deleteBtn, 1);
                            commentContainer.Children.Add(deleteBtn);

                            CommentsDisplayPanel.Children.Add(commentContainer);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loading failed: " + ex.Message);
            }
        }

        private async void DeleteComment_Click(object sender, RoutedEventArgs e) // אירוע לחיצה על כפתור המחיקה של תגובה
        {
            var button = sender as Button; // משיכת הכפתור שנלחץ
            var comment = button?.Tag as VideoReview; 

            if (comment == null) return;
            // הצגת תיבת אישור לפני המחיקה
            var result = MessageBox.Show("Are you sure you want to delete this comment?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int affectedRows = await api.DeleteVideoReview(comment.Id); // קריאה למחיקת התגובה מהשרת

                    if (affectedRows > 0)
                    {
                        // רענון התצוגה של התגובות
                        LoadContent("Comments");
                    }
                    else
                    {
                        MessageBox.Show("Delete failed.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }


        // אירוע לחיצה על כפתור "Show All History" שמציג את כל היסטוריית הצפייה של המשתמש
        private void ShowAllHistory_Click(object sender, RoutedEventArgs e) 
        {
            MainDisplayPanel.Children.Clear();
            FillVideoPanel(fullHistoryList);
            ShowAllHistoryButton.Visibility = Visibility.Collapsed;
        }
        // פונקציה שממלאת את הפאנל הראשי עם תמונות הסרטים בהתאם לרשימת הסרטים שנשלחה לה
        private async void FillVideoPanel(IEnumerable<Video> videos)
        {
            foreach (var v in videos)
            {
                if (v == null) continue;
                if (string.IsNullOrEmpty(v.VideoPic) || v.VideoPic.StartsWith("File"))
                    v.VideoPic = await api.GetVideoPicByte64(v.Id);
                MainDisplayPanel.Children.Add(CreateMovieThumbnail(v));
            }
        }
        // אירוע שינוי הבחירה בתפריט הצדדי שמטעין את התוכן המתאים בהתאם לקטגוריה שנבחרה
        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem is TextBlock selectedItem)
                LoadContent(selectedItem.Tag.ToString());
        }
        // פונקציה שמייצרת כפתור עם תמונת הסרטון שמוביל לדף הפרטים של הסרטון בעת לחיצה
        private Button CreateMovieThumbnail(Video v)
        {
            Button btn = new Button
            {
                Width = 150,
                Height = 220,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            // יצירת תמונה עם עיצוב של מסגרת עגולה
            Image movieImg = new Image { Stretch = Stretch.UniformToFill, Width = 150, Height = 220 };
            try { if (v != null && !string.IsNullOrEmpty(v.VideoPic)) movieImg.Source = Base64ToImage(v.VideoPic); }
            catch { }
            Border mask = new Border { CornerRadius = new CornerRadius(10), ClipToBounds = true, Child = movieImg };
            btn.Content = mask;
            btn.Click += (s, e) => this.NavigationService.Navigate(new MovieDetails(v));
            return btn;
        }
        // אירוע טעינת הדף שמוודא שטעינת התוכן מתבצעת רק לאחר שהדף נטען במלואו
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MenuListBox.SelectedItem is TextBlock selectedItem) LoadContent(selectedItem.Tag.ToString());
            else MenuListBox.SelectedIndex = 0;
        }
        // אירוע לחיצה על כפתור היציאה שמבצע התנתקות של המשתמש ומחזיר לדף הכניסה
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App myApp) myApp.LoggedInUser = null;
            this.NavigationService.Navigate(new NoaMedia.Pages.Log_in());
        }
        
        // אירוע לחיצה על כפתור החזרה שמחזיר לדף הקודם אם קיים
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack) this.NavigationService.GoBack();
        }
        
        // אירוע לחיצה על כפתור השדרוג שמוביל לדף רכישת פרימיום
        private void Upgrade_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PremiumSalesPage());
        }
        
        // פונקציה שממירה מחרוזת בייס64 לתמונה
        public BitmapImage Base64ToImage(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String) || base64String.StartsWith("File")) return null;
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch { return null; }
        }
    }
}