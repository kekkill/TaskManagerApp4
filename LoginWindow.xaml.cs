using TaskManagerApp.Resources;
using MahApps.Metro.Controls;
using System.Windows;
using TaskManagerApp.Models;
using TaskManagerApp.Services;

namespace TaskManagerApp
{
    public partial class LoginWindow : MetroWindow
    {
        private readonly AuthService _authService;
        public User? AuthenticatedUser { get; private set; }

        public LoginWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(Strings.EmptyField,Strings.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = _authService.Authenticate(username, password);
            if (user != null)
            {
                CurrentUser.Instance = user;
                AuthenticatedUser = user;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(Strings.UncorrectNamePasswordMessage, Strings.ErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(_authService);
            registerWindow.ShowDialog();
        }
    }
}