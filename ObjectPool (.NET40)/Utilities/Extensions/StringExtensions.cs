// File name: StringExtensions.cs
// 
// Author(s): Alessio Parma <alessio.parma@gmail.com>
// 
// The MIT License (MIT)
// 
// Copyright (c) 2014-2016 Alessio Parma <alessio.parma@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace CodeProject.ObjectPool.Utilities.Extensions
{
    internal static class StringExtensions
    {
        #region Type parsing with default fallback

        public static bool ToBooleanOrDefault(this string value)
        {
            return ToBooleanOrDefault(value, () => default(bool));
        }

        public static bool ToBooleanOrDefault(this string value, bool defaultValue)
        {
            return ToBooleanOrDefault(value, () => defaultValue);
        }

        public static bool ToBooleanOrDefault(this string value, System.Func<bool> getDefaultValue)
        {
            bool x;
            return System.Boolean.TryParse(value, out x) ? x : getDefaultValue();
        }

        public static TEnum ToEnumOrDefault<TEnum>(this string value) where TEnum : struct
        {
            return ToEnumOrDefault(value, () => default(TEnum));
        }

        public static TEnum ToEnumOrDefault<TEnum>(this string value, TEnum defaultValue) where TEnum : struct
        {
            return ToEnumOrDefault(value, () => defaultValue);
        }

        public static TEnum ToEnumOrDefault<TEnum>(this string value, System.Func<TEnum> getDefaultValue) where TEnum : struct
        {
            TEnum x;
            return System.Enum.TryParse(value, out x) ? x : getDefaultValue();
        }

        public static int ToInt32OrDefault(this string value)
        {
            return ToInt32OrDefault(value, () => default(int));
        }

        public static int ToInt32OrDefault(this string value, int defaultValue)
        {
            return ToInt32OrDefault(value, () => defaultValue);
        }

        public static int ToInt32OrDefault(this string value, System.Func<int> getDefaultValue)
        {
            int x;
            return System.Int32.TryParse(value, out x) ? x : getDefaultValue();
        }

        #endregion Type parsing with default fallback

        #region Web utilities

        /// <summary>
        ///   UrlEncodes a string without the requirement for System.Web
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        // [Obsolete("Use System.Uri.EscapeDataString instead")]
        public static string UrlEncode(this string text)
        {
            // Sytem.Uri provides reliable parsing
            return System.Uri.EscapeDataString(text);
        }

        /// <summary>
        ///   UrlDecodes a string without requiring System.Web
        /// </summary>
        /// <param name="text">String to decode.</param>
        /// <returns>decoded string</returns>
        public static string UrlDecode(this string text)
        {
            // pre-process for + sign space formatting since System.Uri doesn't handle it plus
            // literals are encoded as %2b normally so this should be safe
            text = text.Replace("+", " ");
            return System.Uri.UnescapeDataString(text);
        }

        /// <summary>
        ///   Retrieves a value by key from a UrlEncoded string.
        /// </summary>
        /// <param name="urlEncoded">UrlEncoded String</param>
        /// <param name="key">Key to retrieve value for</param>
        /// <returns>returns the value or "" if the key is not found or the value is blank</returns>
        public static string GetUrlEncodedKey(this string urlEncoded, string key)
        {
            urlEncoded = "&" + urlEncoded + "&";

            var Index = urlEncoded.IndexOf("&" + key + "=", System.StringComparison.OrdinalIgnoreCase);
            if (Index < 0)
            {
                return "";
            }

            var lnStart = Index + 2 + key.Length;

            var Index2 = urlEncoded.IndexOf("&", lnStart, System.StringComparison.OrdinalIgnoreCase);
            if (Index2 < 0)
            {
                return "";
            }

            return UrlDecode(urlEncoded.Substring(lnStart, Index2 - lnStart));
        }

        /// <summary>
        ///   HTML-encodes a string and returns the encoded string.
        /// </summary>
        /// <param name="text">The text string to encode.</param>
        /// <returns>The HTML-encoded text.</returns>
        public static string HtmlEncode(this string text)
        {
            if (text == null)
            {
                return null;
            }

            var sb = new System.Text.StringBuilder(text.Length);

            var len = text.Length;
            for (var i = 0; i < len; i++)
            {
                switch (text[i])
                {
                    case '<':
                        sb.Append("&lt;");
                        break;

                    case '>':
                        sb.Append("&gt;");
                        break;

                    case '"':
                        sb.Append("&quot;");
                        break;

                    case '&':
                        sb.Append("&amp;");
                        break;

                    default:
                        if (text[i] > 159)
                        {
                            // decimal numeric entity
                            sb.Append("&#");
                            sb.Append(((int) text[i]).ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sb.Append(";");
                        }
                        else
                        {
                            sb.Append(text[i]);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        #endregion Web utilities

        #region JavaScript utilities

        public static string ToJavaScriptString(this string s, bool useSingleQuotes)
        {
            var delimiter = useSingleQuotes ? '\'' : '\"';
            var sb = new System.Text.StringBuilder();
            sb.Append(delimiter);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\'':
                        sb.Append("\\\'");
                        break;

                    case '\"':
                        sb.Append("\\\"");
                        break;

                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '\b':
                        sb.Append("\\b");
                        break;

                    case '\f':
                        sb.Append("\\f");
                        break;

                    case '\n':
                        sb.Append("\\n");
                        break;

                    case '\r':
                        sb.Append("\\r");
                        break;

                    case '\t':
                        sb.Append("\\t");
                        break;

                    default:
                        int i = c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append(delimiter);
            return sb.ToString();
        }

        public static string ToJavaScriptString(this string s)
        {
            return ToJavaScriptString(s, false);
        }

        public static string ToJavaScriptString(this char c)
        {
            return ToJavaScriptString(new string(c, 1));
        }

        public static string ToJavaScriptString(this char? c)
        {
            const string jsNull = "null";
            return c.HasValue ? ToJavaScriptString(c.Value) : jsNull;
        }

        public static string ToJQueryID(this string str)
        {
            if (System.String.IsNullOrWhiteSpace(str))
            {
                str = System.String.Empty;
            }
            return System.String.Format("\"#{0}\"", str);
        }

        public static string ToJQueryClass(this string str)
        {
            if (System.String.IsNullOrWhiteSpace(str))
            {
                str = System.String.Empty;
            }
            return System.String.Format("\".{0}\"", str);
        }

        #endregion JavaScript utilities

        public static string Truncate(this string str, int maxLength)
        {
            if (System.String.IsNullOrEmpty(str))
            {
                return str;
            }
            maxLength = System.Math.Max(0, maxLength);
            return (str.Length < maxLength ? str : str.Substring(0, maxLength));
        }
    }
}