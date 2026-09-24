namespace GateOfEgypt.API.Helpers
{
    // Prefixes stored relative file paths (e.g. "/Images/Programs/xyz.jpg") with the API's base URL
    // so clients get a directly usable absolute URL. BaseApiUrl is set once at startup from config.
    public static class UrlHelper
    {
        public static string BaseApiUrl { get; set; } = string.Empty;

        public static string Prefix(string? path) =>
            string.IsNullOrEmpty(path) ? string.Empty : $"{BaseApiUrl}{path}";

        public static List<string> PrefixAll(List<string>? paths) =>
            paths == null || paths.Count == 0 ? new List<string>() : paths.Select(Prefix).ToList();
    }
}
