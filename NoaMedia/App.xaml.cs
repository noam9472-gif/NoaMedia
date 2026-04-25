using Model;
using System.Configuration;
using System.Data;
using System.Windows;

namespace NoaMedia
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // המשתנה שיחזיק את המשתמש המחובר מכל מקום בפרויקט
        public Model.User LoggedInUser { get; set; }

        // משתנה סטטי שנגיש מכל מקום בפרויקט
        public static User CurrentUser { get; set; }

    }
}
