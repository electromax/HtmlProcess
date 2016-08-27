using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace HtmlProcess
{
    public static class HtmlTransformer
    {
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

    }
}
