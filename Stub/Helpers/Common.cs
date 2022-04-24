namespace Stealerium.Helpers
{
    public struct Password
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Pass { get; set; }
    }

    public struct Cookie
    {
        public string HostKey { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ExpiresUtc { get; set; }
        public string Value { get; set; }
        public string IsSecure { get; set; }
    }

    public struct CreditCard
    {
        public string Number { get; set; }
        public string ExpYear { get; set; }
        public string ExpMonth { get; set; }
        public string Name { get; set; }
    }

    public struct AutoFill
    {
        public string Name;
        public string Value;
    }

    public struct Site
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
    }

    public struct Bookmark
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }
}