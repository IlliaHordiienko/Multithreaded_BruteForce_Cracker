using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BruteForceCracker
{
    public partial class MainWindow : Window
    {
        private string _targetHash = string.Empty;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string secretPassword = PasswordManager.GenerateRandomPassword();
            _targetHash = PasswordManager.ComputeHash(secretPassword);

            TxtTargetInfo.Text = $"Target set! Length: {secretPassword.Length} | Hash: {_targetHash.Substring(0, 15)}...";
            TxtTargetInfo.Foreground = System.Windows.Media.Brushes.DarkGreen;
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_targetHash))
            {
                MessageBox.Show("Please generate a secret password first.", "System Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;
            TxtLog.Text = "Initiating performance benchmarks...\n\n";
            _cts = new CancellationTokenSource();

            BruteForceGenerator generator = new BruteForceGenerator();
            PasswordValidator validator = new PasswordValidator(_targetHash);

            // Synchronizes operational metric updates back to the UI layout thread
            Action<string, long> reportProgress = (currentGuess, totalAttempts) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TxtCount.Text = totalAttempts.ToString("N0");
                });
            };

            // Execution sequence for single-threaded performance testing
            TxtLog.AppendText("[Test 1/2] Launching single-threaded brute force...\n");
            Stopwatch sw = Stopwatch.StartNew();

            var timerToken = _cts.Token;
            _ = Task.Run(async () =>
            {
                while (!timerToken.IsCancellationRequested && sw.IsRunning)
                {
                    Dispatcher.Invoke(() => TxtTime.Text = sw.Elapsed.ToString(@"hh\:mm\:ss\.fff"));
                    await Task.Delay(45);
                }
            });

            string singleResult = null;
            TimeSpan singleTime = TimeSpan.Zero;

            try
            {
                singleResult = await Task.Run(() => generator.RunSingleThreaded(validator, reportProgress, _cts.Token));
                sw.Stop();
                singleTime = sw.Elapsed;
                TxtLog.AppendText($"-> Completed. Time: {singleTime.TotalMilliseconds:F2} ms. Target: {singleResult ?? "Aborted"}\n\n");
            }
            catch (Exception ex)
            {
                TxtLog.AppendText($"Single-threaded execution error: {ex.Message}\n\n");
            }

            if (_cts.IsCancellationRequested)
            {
                ResetUiState();
                return;
            }

            // Flushes visual state variables prior to running parallel loop test
            TxtCount.Text = "0";
            TxtTime.Text = "00:00:00.000";

            // Execution sequence for multi-threaded performance testing
            TxtLog.AppendText("[Test 2/2] Launching multi-threaded parallel execution...\n");
            sw.Restart();

            string multiResult = null;
            TimeSpan multiTime = TimeSpan.Zero;

            try
            {
                multiResult = await Task.Run(() => generator.RunMultiThreaded(validator, reportProgress, _cts.Token));
                sw.Stop();
                multiTime = sw.Elapsed;
                TxtLog.AppendText($"-> Completed. Time: {multiTime.TotalMilliseconds:F2} ms. Target: {multiResult ?? "Aborted"}\n\n");
            }
            catch (Exception ex)
            {
                TxtLog.AppendText($"Multi-threaded execution error: {ex.Message}\n\n");
            }

            // Calculates execution speedups and logs comprehensive metric summaries
            if (multiResult != null)
            {
                TxtResult.Text = multiResult;
                double speedup = singleTime.TotalMilliseconds / Math.Max(1, multiTime.TotalMilliseconds);

                TxtLog.AppendText("=========================================\n");
                TxtLog.AppendText("         FINAL PERFORMANCE LOGS          \n");
                TxtLog.AppendText("=========================================\n");
                TxtLog.AppendText($"Single-Thread Duration: {singleTime.ToString(@"ss\.fff")} seconds\n");
                TxtLog.AppendText($"Multi-Thread Duration : {multiTime.ToString(@"ss\.fff")} seconds\n");
                TxtLog.AppendText($"Hardware Performance Acceleration: {speedup:F2}x faster\n");
                TxtLog.AppendText("=========================================\n");
            }

            ResetUiState();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            ResetUiState();
            TxtResult.Text = "Stopped.";
            TxtLog.AppendText("\nOperation explicitly cancelled by user configuration.\n");
        }

        private void ResetUiState()
        {
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }
    }
}