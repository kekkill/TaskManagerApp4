using TaskManagerApp.Resources;
using MahApps.Metro.Controls;
using System.Windows;
using TaskManagerApp.Data;
using TaskManagerApp.Models;

namespace TaskManagerApp
{
    public partial class InputDialog : MetroWindow
    {
        public string InputText => InputTextBox.Text;
        public int? SelectedWorkerId => WorkersComboBox.SelectedItem is User user ? user.Id : (int?)null;

        private readonly AppDbContext _context;
        private readonly bool _showWorkers;

        public InputDialog(string title, string label, User currentUser = null, bool forTask = true)
        {
            InitializeComponent();
            Title = title;
            TitleText.Text = label;
            _context = new AppDbContext();

            _showWorkers = (currentUser != null &&
                           (currentUser.Role == "Admin" || currentUser.Role == "Manager") &&
                           forTask);

            if (_showWorkers)
            {
                WorkerPanel.Visibility = Visibility.Visible;
                LoadWorkers();
            }
        }

        private void LoadWorkers()
        {
            var workers = _context.Users
                .Where(u => u.Role == "Worker")
                .OrderBy(u => u.Username)
                .ToList();

            WorkersComboBox.ItemsSource = workers;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                MessageBox.Show(Strings.EmptyField, Strings.Warning,
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_showWorkers && WorkersComboBox.SelectedItem == null)
            {
                MessageBox.Show(Strings.ChooseWorker, Strings.Warning,
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}