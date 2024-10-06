namespace Stealerium.Stub.Helpers
{
    /// <summary>
    /// Represents a saved password entry for a website.
    /// </summary>
    public class Password
    {
        /// <summary>
        /// Gets or sets the URL of the website.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the username associated with the password.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Pass { get; set; }
    }

    /// <summary>
    /// Represents a cookie entry.
    /// </summary>
    public class Cookie
    {
        /// <summary>
        /// Gets or sets the host key for the cookie (domain).
        /// </summary>
        public string HostKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the cookie.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the cookie (URL path).
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the cookie in UTC format.
        /// </summary>
        public string ExpiresUtc { get; set; }

        /// <summary>
        /// Gets or sets the value of the cookie.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets whether the cookie is secure (HTTPS only).
        /// </summary>
        public string IsSecure { get; set; }
    }

    /// <summary>
    /// Represents a credit card entry.
    /// </summary>
    public class CreditCard
    {
        /// <summary>
        /// Gets or sets the credit card number.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the expiration year of the credit card.
        /// </summary>
        public string ExpYear { get; set; }

        /// <summary>
        /// Gets or sets the expiration month of the credit card.
        /// </summary>
        public string ExpMonth { get; set; }

        /// <summary>
        /// Gets or sets the cardholder's name.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents an autofill entry, such as saved form data.
    /// </summary>
    public struct AutoFill
    {
        /// <summary>
        /// Gets or sets the name of the form field (e.g., 'First Name').
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value entered into the form field.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Represents a frequently visited site entry.
    /// </summary>
    public class Site
    {
        /// <summary>
        /// Gets or sets the URL of the frequently visited site.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the title of the website.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the number of times the site has been visited.
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Represents a bookmark entry.
    /// </summary>
    public class Bookmark
    {
        /// <summary>
        /// Gets or sets the URL of the bookmarked site.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the title of the bookmarked page.
        /// </summary>
        public string Title { get; set; }
    }
}
