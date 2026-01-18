using System.Windows;
using System.Windows.Controls;

namespace App4
{
    public partial class DialogButtonPanel : UserControl
    {
        public DialogButtonPanel()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler OkClicked;
        public event RoutedEventHandler CancelClicked;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OkClicked?.Invoke(this, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, e);
        }
    }
}