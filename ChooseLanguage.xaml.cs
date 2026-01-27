using TaskManagerApp.Data;
using TaskManagerApp.Services;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows;

namespace TaskManagerApp
{
    /// <summary>
    /// Логика взаимодействия для ChooseLanguage.xaml
    /// </summary>
    public partial class ChooseLanguage : MetroWindow
    {
        public string SelectedLanguage { get; private set; } = "ru-RU";

        public ChooseLanguage()
        {
            InitializeComponent();
            LanguageComboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string selectedLanguage = LanguageComboBox.SelectedIndex == 0 ? "ru-RU" : "en-US";
            var culture = new CultureInfo(selectedLanguage);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;

            using (var context = new AppDbContext())
            {
                context.Database.Migrate();

                var authService = new AuthService(context);

                var loginWindow = new LoginWindow(authService);
                bool isLoggedIn = loginWindow.ShowDialog() == true;
                if (isLoggedIn && loginWindow.AuthenticatedUser != null)
                {
                    var mainWindow = new MainWindow(loginWindow.AuthenticatedUser);
                    mainWindow.Show(); 

                    Close();
                }
                else
                {
                    Close();
                }
            } 
        }
    }
}
