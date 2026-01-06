using System.Windows;

namespace NoamediaLogin
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        // לוגיקה לכפתור ההרשמה
        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewUserTextBox.Text) || string.IsNullOrEmpty(NewPasswordBox.Password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // כאן תוכל להוסיף קוד לשמירת המשתמש
            MessageBox.Show("Account created successfully!");

            // חזרה למסך הכניסה
            MainWindow loginWin = new MainWindow();
            loginWin.Show();
            this.Close();
        }

        // לוגיקה לכפתור חזרה
        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWin = new MainWindow();
            loginWin.Show();
            this.Close();
        }
    }
}