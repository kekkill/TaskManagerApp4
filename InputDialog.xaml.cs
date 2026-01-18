using System.Windows;
using MahApps.Metro.Controls;

namespace App4
{
    public partial class InputDialog : MetroWindow
    {
        public string InputText { get; set; }

        public InputDialog(string title, string prompt)
        {
            InitializeComponent();
            TitleText.Text = prompt;
            Title = title;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
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