namespace Stealerium.Helpers
{
    // Represents a saved password entry
    public class Password
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Pass { get; set; }
    }

    // Represents a cookie entry
    public class Cookie
    {
        public string HostKey { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ExpiresUtc { get; set; }
        public string Value { get; set; }
        public string IsSecure { get; set; }
    }

    // Represents a credit card entry
    public class CreditCard
    {
        public string Number { get; set; }
        public string ExpYear { get; set; }
        public string ExpMonth { get; set; }
        public string Name { get; set; }
    }

    // Represents an autofill entry (e.g., form data)
    public struct AutoFill
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    // Represents a frequently visited site entry
    public class Site
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
    }

    // Represents a bookmark entry
    public class Bookmark
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }
}
