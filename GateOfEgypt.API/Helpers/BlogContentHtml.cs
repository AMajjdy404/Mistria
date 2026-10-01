using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace GateOfEgypt.API.Helpers
{
    // Blog sub content values are rich text (HTML) written in the dashboard's editor and rendered as HTML on the website.
    // Every value goes through Format: HTML is sanitized down to safe formatting markup (no scripts, event handlers,
    // javascript: links, ...), and plain text (older entries, or text sent without the editor) is converted to HTML
    // so its line breaks, numbering and bullets still show up instead of collapsing into one paragraph.
    public static class BlogContentHtml
    {
        private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

        // An opening/closing tag such as <p>, </li> or <br/>. Requires a letter right after "<" so text like "a < b" stays plain text.
        private static readonly Regex HtmlTag = new Regex(@"</?[a-zA-Z][a-zA-Z0-9]*\b[^>]*>", RegexOptions.Compiled);

        // Blank line(s) between paragraphs in plain text
        private static readonly Regex ParagraphBreak = new Regex(@"\n\s*\n", RegexOptions.Compiled);

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedTags.Clear();
            foreach (var tag in new[]
            {
                "p", "br", "hr", "div", "span",
                "h1", "h2", "h3", "h4", "h5", "h6",
                "strong", "b", "em", "i", "u", "s", "strike", "del", "mark", "sub", "sup", "small",
                "blockquote", "pre", "code",
                "ul", "ol", "li",
                "a", "img", "figure", "figcaption",
                "table", "thead", "tbody", "tfoot", "tr", "th", "td"
            })
                sanitizer.AllowedTags.Add(tag);

            sanitizer.AllowedAttributes.Clear();
            foreach (var attribute in new[] { "href", "target", "rel", "title", "src", "alt", "width", "height", "start", "colspan", "rowspan", "dir", "class", "style" })
                sanitizer.AllowedAttributes.Add(attribute);

            // Editors use inline styles / classes for alignment and indentation only
            sanitizer.AllowedCssProperties.Clear();
            foreach (var property in new[] { "text-align", "direction", "padding-left", "padding-right", "margin-left", "margin-right" })
                sanitizer.AllowedCssProperties.Add(property);

            sanitizer.AllowedSchemes.Clear();
            foreach (var scheme in new[] { "http", "https", "mailto", "tel" })
                sanitizer.AllowedSchemes.Add(scheme);

            // Links opened in a new tab must not get access to window.opener
            sanitizer.PostProcessNode += (_, e) =>
            {
                if (e.Node is AngleSharp.Dom.IElement element
                    && element.TagName.Equals("A", StringComparison.OrdinalIgnoreCase)
                    && element.GetAttribute("target") == "_blank")
                {
                    element.SetAttribute("rel", "noopener noreferrer");
                }
            };

            return sanitizer;
        }

        public static Dictionary<string, string> Format(Dictionary<string, string>? content)
        {
            if (content == null)
                return new Dictionary<string, string>();

            return content.ToDictionary(kv => kv.Key, kv => Format(kv.Value));
        }

        public static string Format(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return HtmlTag.IsMatch(value) ? Sanitizer.Sanitize(value) : PlainTextToHtml(value);
        }

        // Blank lines start a new <p>; single line breaks become <br>. Text is HTML-encoded.
        private static string PlainTextToHtml(string text)
        {
            var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();

            var paragraphs = ParagraphBreak.Split(normalized)
                .Select(p => string.Join("<br>", p.Split('\n').Select(line => WebUtility.HtmlEncode(line.TrimEnd()))))
                .Select(p => $"<p>{p}</p>");

            return string.Concat(paragraphs);
        }
    }
}
