using MahApps.Metro.Controls;
using System.Windows;
using TaskManagerApp.Services;
using TaskManagerApp.Resources;

namespace TaskManagerApp
{
    public partial class RegisterWindow : MetroWindow
    {
        private readonly AuthService _authService;

        public RegisterWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(Strings.RegisterWindow_FieldError, Strings.ErrorMessage, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show(Strings.RegisterWindow_SymbolError, Strings.ErrorMessage, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_authService.RegisterUser(username, password, email))
                {
                    MessageBox.Show(Strings.RegisterWindow_SuccesAuth, Strings.RegisterWindow_Success, MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show(Strings.RegisterWindow_UserExists, Strings.ErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(Strings.RegisterWindow_RegisterFail + $"{ex.Message}", Strings.ErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}