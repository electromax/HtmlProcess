using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlProcess
{
    public static class HtmlTextExtractor
    {
        static readonly HashSet<string> InlineTags = new HashSet<string>
        {
          "b", "big", "i", "small", "tt", "abbr", "acronym",
          "cite", "dfn", "em", "kbd", "strong", "samp", "font",
          "var", "a", "bdo", "q", "span", "sub", "sup", "label", "code", "td", "th"
        };
        static readonly HashSet<string> NonVisibleTags = new HashSet<string>
        {
            "script", "style", "map", "object", "img", "button", "input", "select", "textarea"
        };
        enum ToPlainTextState
        {
            StartLine = 0,
            NotWhiteSpace,
            WhiteSpace,
        }

        public static StringBuilder GetStringBuilder(string html)
        {
            var builder = new StringBuilder();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            try
            {
                doc.LoadHtml(html);
                var state = ToPlainTextState.StartLine;
                PlainTextRecursive(builder, ref state, doc.DocumentNode.ChildNodes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            doc = null;
            return builder;
        }

        public static string Get(string html)
        {
            return GetStringBuilder(html).ToString();
        }

        static void PlainTextRecursive(StringBuilder builder, ref ToPlainTextState state, IEnumerable<HtmlAgilityPack.HtmlNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is HtmlAgilityPack.HtmlTextNode)
                {
                    var text = (HtmlAgilityPack.HtmlTextNode)node;
                    PlainTextBuild(builder, ref state, System.Net.WebUtility.HtmlDecode(text.Text));
                }
                else
                {
                    var tag = node.Name;
                    if (tag == "br")
                    {
                        builder.AppendLine();
                        state = ToPlainTextState.StartLine;
                    }
                    else if (tag == "pre")
                    {
                        if (state != ToPlainTextState.StartLine)
                        {
                            builder.AppendLine();
                        }
                        builder.Append(System.Net.WebUtility.HtmlDecode(node.InnerHtml));
                        builder.AppendLine();
                        state = ToPlainTextState.StartLine;
                    }
                    else if (NonVisibleTags.Contains(tag))
                    {
                    }
                    else if (InlineTags.Contains(tag))
                    {
                        PlainTextRecursive(builder, ref state, node.ChildNodes);
                        if (tag == "td" || tag == "th")
                            builder.Append('\t');
                    }
                    else
                    {
                        if (state != ToPlainTextState.StartLine)
                        {
                            builder.AppendLine();
                            state = ToPlainTextState.StartLine;
                        }
                        PlainTextRecursive(builder, ref state, node.ChildNodes);
                        if (state != ToPlainTextState.StartLine)
                        {
                            builder.AppendLine();
                            state = ToPlainTextState.StartLine;
                        }
                    }
                }
            }
        }

        static void PlainTextBuild(StringBuilder builder, ref ToPlainTextState state, string chars)
        {
            for (var i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if (char.IsWhiteSpace(ch))
                {
                    if (IsHardSpace(ch))
                    {
                        if (state == ToPlainTextState.WhiteSpace)
                            builder.Append(' ');
                        builder.Append(' ');
                        state = ToPlainTextState.NotWhiteSpace;
                    }
                    else
                    {
                        if (state == ToPlainTextState.NotWhiteSpace)
                            state = ToPlainTextState.WhiteSpace;
                    }
                }
                else
                {
                    if (state == ToPlainTextState.WhiteSpace)
                        builder.Append(' ');
                    builder.Append(ch);
                    state = ToPlainTextState.NotWhiteSpace;
                }
            }
        }

        static bool IsHardSpace(char ch)
        {
            return ch == 0xA0 || ch == 0x2007 || ch == 0x202F;
        }
    }
}
