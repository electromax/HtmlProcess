using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace HtmlProcess
{
    public static class HttpFetcher
    {
        public static HttpWebRequest CreateRequest(string url, int timeout)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Timeout = timeout;
            request.AllowAutoRedirect = true;
            return request;
        }

        public static string Fetch(string url, int timeout, out string respondedUrl, out string contentType)
        {
            var request = CreateRequest(url, timeout);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                respondedUrl = request.Address.ToString();
                contentType = response.ContentType;
                using (var stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static MemoryStream FetchToStream(string url, int timeout, out string respondedUrl, out string contentType)
        {
            var result = new MemoryStream();
            var request = CreateRequest(url, timeout);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                respondedUrl = request.Address.ToString();
                contentType = response.ContentType;
                using (var stream = response.GetResponseStream())
                {
                    stream.CopyTo(result);
                }
            }
            result.Position = 0;
            return result;
        }

        public static string ReplaceLinks(string html, string respondedUrl)
        {
            var parts = respondedUrl.Split('/');
            var domain = string.Join("/", parts.Take(3));
            var folder = string.Join("/", parts.Take(parts.Length - 1)) + '/';
            var matchEvaluator = new MatchEvaluator(
                match =>
                {
                    var href = WebUtility.HtmlDecode(Uri.UnescapeDataString(match.Groups["value"].Value.Replace("+", " "))).Trim();
                    if (href.StartsWith("//"))
                    {
                        href = parts[0] + href;
                    }
                    else if (href.StartsWith("/"))
                    {
                        href = domain + href;
                    }
                    else if (!href.StartsWith("http:") && !href.StartsWith("https:") && !href.StartsWith("ftp:")
                        && !href.StartsWith("javascript:") && !href.StartsWith("#") && !href.StartsWith("mailto:"))
                    {
                        href = folder + href;
                    }
                    return string.Format("{0}{2}{1}{2}", match.Groups["name"].Value, href, match.Groups["separator"].Value);
                });
            html = Regex.Replace(html, "<a([^>]*)href=\"#[^\"]*\"([^>]*)rel=(\"[^\"]*\")", "<a $1 $2 href=$3");
            html = Regex.Replace(html, "<a([^>]*)href='#[^']*'([^>]*)rel=(\"[^\"]*\")", "<a $1 $2 href=$3");
            html = Regex.Replace(html, "<a([^>]*)href=\"#[^\"]*\"([^>]*)rel=('[^']*')", "<a $1 $2 href=$3");
            html = Regex.Replace(html, "<a([^>]*)href='#[^']*'([^>]*)rel=('[^']*')", "<a $1 $2 href=$3");
            html = Regex.Replace(html, @"(?<name>src=|href=|data-responsive-image-default=|@import\s*)""(?<value>[^""]*)(?<separator>"")", matchEvaluator, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"(?<name>src=|href=|data-responsive-image-default=|@import\s*)'(?<value>[^']*)(?<separator>')", matchEvaluator, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"(?<name>@import\s*)url\(""(?<value>[^""]*)(?<separator>"")\)", matchEvaluator, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"(?<name>@import\s*)url\('(?<value>[^']*)(?<separator>')\)", matchEvaluator, RegexOptions.IgnoreCase);
            return html;
        }
    }
}
