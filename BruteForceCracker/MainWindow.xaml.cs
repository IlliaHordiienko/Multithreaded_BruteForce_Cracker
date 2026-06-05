using System.Windows;

namespace BruteForceCracker
{
    public partial class MainWindow : Window
    {
        // Stores targeted hash value to crack
        private string _targetHash = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string secretPassword = PasswordManager.GenerateRandomPassword();
            _targetHash = PasswordManager.ComputeHash(secretPassword);

            // Updates display with length and truncated hash information
            TxtTargetInfo.Text = $"Target set! Length: {secretPassword.Length} | Hash: {_targetHash.Substring(0, 15)}...";
            TxtTargetInfo.Foreground = System.Windows.Media.Brushes.DarkGreen;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;
            TxtResult.Text = "Searching...";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
            TxtResult.Text = "Stopped.";
        }
    }
}