using System;

namespace Stealerium.Stub.Target.Browsers.Common.Models
{
    public class BrowserCookie
    {
        public string HostKey { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
        public string IsSecure { get; set; }
        public string ExpiresUtc { get; set; }
    }

    public class BrowserBookmark
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }

    public class BrowserAutofill
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class BrowserCreditCard
    {
        public string Number { get; set; }
        public string ExpYear { get; set; }
        public string ExpMonth { get; set; }
        public string Name { get; set; }
    }

    public class BrowserHistoryItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public DateTime VisitTime { get; set; }
        public int VisitCount { get; set; }
    }

    public class BrowserDownload
    {
        public string TargetPath { get; set; }
        public string Url { get; set; }
        public long TotalBytes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class BrowserPassword
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
    }
}
