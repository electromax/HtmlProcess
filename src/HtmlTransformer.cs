using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace HtmlProcess
{
    public static class HtmlTransformer
    {
        public static string SetBaseUrlForSavedHtml(string html, string respondedUrl)
        {
            var parts = respondedUrl.Split('/');
            var baseTagMatch = Regex.Match(html, "<base\\s[^>]*href=['\"]([^'\">]*)['\"][^>]*>", RegexOptions.IgnoreCase);
            if (baseTagMatch.Success)
            {
                var baseUrl = baseTagMatch.Groups[1].Value;
                baseUrl = WebUtility.HtmlDecode(Uri.UnescapeDataString(baseUrl)).Trim();
                if (baseUrl.Contains(':'))
                {
                    parts = baseUrl.Split('/');
                }
                else if (baseUrl.StartsWith("//"))
                {
                    baseUrl = parts[0] + baseUrl;
                    parts = baseUrl.Split('/');
                }
                else if (baseUrl.StartsWith("/"))
                {
                    baseUrl = string.Join("/", parts.Take(3)) + baseUrl;
                    parts = baseUrl.Split('/');
                }
                else if (baseUrl != "" && !baseUrl.StartsWith("#"))
                {
                    baseUrl = string.Join("/", parts.Take(parts.Length - 1)) + "/" + baseUrl;
                    parts = baseUrl.Split('/');
                }
                html = html.Remove(baseTagMatch.Index, baseTagMatch.Length);
            }
            var idxToInsert = html.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
            if (idxToInsert >= 0)
                idxToInsert = html.IndexOf('>', idxToInsert);
            if (idxToInsert < 0)
                idxToInsert = html.IndexOf('>');
            if (idxToInsert < 0)
                return html;
            idxToInsert++;
            return html.Substring(0, idxToInsert) + "<base href='" + string.Join("/", parts.Take(parts.Length - 1)) + "/'>" + html.Remove(0, idxToInsert);
        }

        public static string RebaseUrls(string html, string respondedUrl)
        {
            var parts = respondedUrl.Split('/');
            var matchEvaluator = new MatchEvaluator(
                match =>
                {
                    var href = WebUtility.HtmlDecode(Uri.UnescapeDataString(match.Groups["value"].Value)).Trim();
                    if (href.StartsWith("//"))
                    {
                        href = parts[0] + href;
                    }
                    else if (href.StartsWith("/"))
                    {
                        href = string.Join("/", parts.Take(3)) + href;
                    }
                    else if (href != "" && !href.Contains(':') && !href.StartsWith("#"))
                    {
                        href = string.Join("/", parts.Take(parts.Length - 1)) + "/" + href;
                    }
                    return string.Format("{0}{2}{1}{2}", match.Groups["name"].Value, href, match.Groups["separator"].Value);
                });
            html = Regex.Replace(html, @"(?<name>src=|href=|data-responsive-image-default=|@import\s*)""(?<value>[^""]*)(?<separator>"")", matchEvaluator, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"(?<name>src=|href=|data-responsive-image-default=|@import\s*)'(?<value>[^']*)(?<separator>')", matchEvaluator, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"(?<name>url\()""(?<value>[^""]*)(?<separator>"")", matchEvaluator, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"(?<name>url\()'(?<value>[^']*)(?<separator>')", matchEvaluator, RegexOptions.IgnoreCase);
            return html;
        }
    }
}
