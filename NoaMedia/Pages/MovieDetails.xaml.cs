using ApiInterface;
using Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NoaMedia.Pages
{
    public partial class MovieDetails : Page
    {
        private InterfaceAPI api = new InterfaceAPI();
        private Video currentVideo; 

        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();
            currentVideo = selectedVideo; // שמירת הסרט שנבחר
            LoadMovieDetails(selectedVideo);
        }

        private async void LoadMovieDetails(Video v)
        {
            MovieTitle.Text = v.VideoName;
            MovieGenre.Text = v.Genre?.GenreDescription;
            MovieDuration.Text = v.LengthInMinutes + " min";

            // תקציר קצר- שם הסרט
            MovieDesc.Text = v.VideoName;

            // תקציר מלא
            FullDescriptionText.Text = string.IsNullOrEmpty(v.VideoDescription) ? "No description available." : v.VideoDescription;

            
            if (WhoUploadedName != null)
            {
                WhoUploadedName.Text = v.WhoUploadedTheVideo?.UserName ?? "Unknown User";
            }

            if (v.VideoUploadedDate != DateTime.MinValue)
            {
                ReleaseYear.Text = v.VideoUploadedDate.ToString("dd/MM/yyyy");
            }
            else
            {
                ReleaseYear.Text = "Unknown Date";
            }

            try
            {
                string base64 = await api.GetVideoPicByte64(v.Id);
                if (!string.IsNullOrEmpty(base64))
                {
                    BackgroundImage.Source = ByteImageConverter.ByteToImage(Convert.FromBase64String(base64));
                }
            }
            catch { }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //בדיקה אם  קיים
                if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress))
                {
                    MessageBox.Show("Error: Video address is missing in the database.", "Missing Data");
                    return;
                }

                string address = currentVideo.VideoAddress;

                if (!address.Contains("://") && !System.IO.Path.IsPathRooted(address))
                {
                    address = "http://" + address;
                }

                Uri videoUri;
                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out videoUri))
                {
                    VideoLayer.Visibility = Visibility.Visible;
                    InlinePlayer.Source = videoUri;
                    InlinePlayer.Play();
                }
                else
                {
                    MessageBox.Show("Error: Invalid URL format.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical Error: " + ex.Message);
            }
        }
        private void CloseVideo_Click(object sender, RoutedEventArgs e)
        {
            InlinePlayer.Stop();
            VideoLayer.Visibility = Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
    {
        
        double scrollTo = MainScrollViewer.ScrollableHeight;

        // יצירת האנימציה - נמשכת 0.8 שניות עם אפקט האטה בסוף (EaseOut)
        DoubleAnimation scrollAnimation = new DoubleAnimation
        {
            From = MainScrollViewer.VerticalOffset,
            To = scrollTo,
            Duration = new Duration(TimeSpan.FromSeconds(0.8)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        // הפעלת האנימציה 
        this.BeginAnimation(ScrollOffsetProperty, scrollAnimation);
    }

    // ---  שמאפשרת לאנימציה לעבוד ScrollViewer ---
    public static readonly DependencyProperty ScrollOffsetProperty =
        DependencyProperty.Register("ScrollOffset", typeof(double), typeof(MovieDetails),
        new PropertyMetadata(0.0, OnScrollOffsetChanged));

    private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var page = d as MovieDetails;
        page?.MainScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
    }
}
}