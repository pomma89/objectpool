using System;
using System.Collections.Generic;
using System.Text;

namespace CodeProject.ObjectPool.Text
{
    /// <summary>
    ///   FastReplacer is a utility class similar to StringBuilder, with fast Replace function.
    ///   FastReplacer is limited to replacing only properly formatted tokens. Use ToString()
    ///   function to get the final text.
    /// </summary>
    /// <remarks>Taken here: http://www.codeproject.com/Articles/298519/Fast-Token-Replacement-in-Csharp</remarks>
    internal sealed class FastReplacer
    {
        private readonly Dictionary<string, List<TokenOccurrence>> OccurrencesOfToken;
        private readonly FastReplacerSnippet _rootSnippet = new FastReplacerSnippet("");
        private readonly string _tokenClose;
        private readonly string _tokenOpen;

        /// <summary>
        ///   All tokens that will be replaced must have same opening and closing delimiters, such
        ///   as "{" and "}".
        /// </summary>
        /// <param name="tokenOpen">Opening delimiter for tokens.</param>
        /// <param name="tokenClose">Closing delimiter for tokens.</param>
        /// <param name="caseSensitive">
        ///   Set caseSensitive to false to use case-insensitive search when replacing tokens.
        /// </param>
        public FastReplacer(string tokenOpen, string tokenClose, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(tokenOpen) || string.IsNullOrEmpty(tokenClose))
            {
                throw new ArgumentException("Token must have opening and closing delimiters, such as \"{\" and \"}\".");
            }

            _tokenOpen = tokenOpen;
            _tokenClose = tokenClose;

            var stringComparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            OccurrencesOfToken = new Dictionary<string, List<TokenOccurrence>>(stringComparer);
        }

        public FastReplacer Append(string text)
        {
            var snippet = new FastReplacerSnippet(text);
            _rootSnippet.Append(snippet);
            ExtractTokens(snippet);
            return this;
        }

        /// <returns>Returns true if the token was found, false if nothing was replaced.</returns>
        public FastReplacer Replace(string token, string text)
        {
            bool result;
            return Replace(token, text, out result);
        }

        /// <returns>Returns true if the token was found, false if nothing was replaced.</returns>
        public FastReplacer Replace(string token, string text, out bool result)
        {
            ValidateToken(token, text, false);
            List<TokenOccurrence> occurrences;
            if (OccurrencesOfToken.TryGetValue(token, out occurrences) && occurrences.Count > 0)
            {
                OccurrencesOfToken.Remove(token);
                var snippet = new FastReplacerSnippet(text);
                foreach (var occurrence in occurrences)
                {
                    occurrence.Snippet.Replace(occurrence.Start, occurrence.End, snippet);
                }
                ExtractTokens(snippet);
                result = true;
                return this;
            }
            result = false;
            return this;
        }

        /// <returns>Returns true if the token was found, false if nothing was replaced.</returns>
        public bool InsertBefore(string token, string text)
        {
            ValidateToken(token, text, false);
            List<TokenOccurrence> occurrences;
            if (OccurrencesOfToken.TryGetValue(token, out occurrences) && occurrences.Count > 0)
            {
                var snippet = new FastReplacerSnippet(text);
                foreach (var occurrence in occurrences)
                {
                    occurrence.Snippet.InsertBefore(occurrence.Start, snippet);
                }
                ExtractTokens(snippet);
                return true;
            }
            return false;
        }

        /// <returns>Returns true if the token was found, false if nothing was replaced.</returns>
        public bool InsertAfter(string token, string text)
        {
            ValidateToken(token, text, false);
            List<TokenOccurrence> occurrences;
            if (OccurrencesOfToken.TryGetValue(token, out occurrences) && occurrences.Count > 0)
            {
                var snippet = new FastReplacerSnippet(text);
                foreach (var occurrence in occurrences)
                {
                    occurrence.Snippet.InsertAfter(occurrence.End, snippet);
                }
                ExtractTokens(snippet);
                return true;
            }
            return false;
        }

        public bool Contains(string token)
        {
            ValidateToken(token, token, false);
            List<TokenOccurrence> occurrences;
            if (OccurrencesOfToken.TryGetValue(token, out occurrences))
            {
                return occurrences.Count > 0;
            }
            return false;
        }

        private void ExtractTokens(FastReplacerSnippet snippet)
        {
            var last = 0;
            while (last < snippet.Text.Length)
            {
                // Find next token position in snippet.Text:
                var start = snippet.Text.IndexOf(_tokenOpen, last, StringComparison.OrdinalIgnoreCase);
                if (start == -1)
                {
                    return;
                }
                var end = snippet.Text.IndexOf(_tokenClose, start + _tokenOpen.Length, StringComparison.OrdinalIgnoreCase);
                if (end == -1)
                {
                    throw new ArgumentException(string.Format("Token is opened but not closed in text \"{0}\".", snippet.Text));
                }
                var eol = snippet.Text.IndexOf('\n', start + _tokenOpen.Length);
                if (eol != -1 && eol < end)
                {
                    last = eol + 1;
                    continue;
                }

                // Take the token from snippet.Text:
                end += _tokenClose.Length;
                var token = snippet.Text.Substring(start, end - start);
                var context = snippet.Text;
                ValidateToken(token, context, true);

                // Add the token to the dictionary:
                var tokenOccurrence = new TokenOccurrence { Snippet = snippet, Start = start, End = end };
                List<TokenOccurrence> occurrences;
                if (OccurrencesOfToken.TryGetValue(token, out occurrences))
                {
                    occurrences.Add(tokenOccurrence);
                }
                else
                {
                    OccurrencesOfToken.Add(token, new List<TokenOccurrence> { tokenOccurrence });
                }

                last = end;
            }
        }

        private void ValidateToken(string token, string context, bool alreadyValidatedStartAndEnd)
        {
            if (!alreadyValidatedStartAndEnd)
            {
                if (!token.StartsWith(_tokenOpen, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Format("Token \"{0}\" shoud start with \"{1}\". Used with text \"{2}\".", token, _tokenOpen, context));
                }
                var closePosition = token.IndexOf(_tokenClose, StringComparison.OrdinalIgnoreCase);
                if (closePosition == -1)
                {
                    throw new ArgumentException(string.Format("Token \"{0}\" should end with \"{1}\". Used with text \"{2}\".", token, _tokenClose, context));
                }
                if (closePosition != token.Length - _tokenClose.Length)
                {
                    throw new ArgumentException(string.Format("Token \"{0}\" is closed before the end of the token. Used with text \"{1}\".", token, context));
                }
            }

            if (token.Length == _tokenOpen.Length + _tokenClose.Length)
            {
                throw new ArgumentException(string.Format("Token has no body. Used with text \"{0}\".", context));
            }
            if (token.Contains("\n"))
            {
                throw new ArgumentException(string.Format("Unexpected end-of-line within a token. Used with text \"{0}\".", context));
            }
            if (token.IndexOf(_tokenOpen, _tokenOpen.Length, StringComparison.OrdinalIgnoreCase) != -1)
            {
                throw new ArgumentException(string.Format("Next token is opened before a previous token was closed in token \"{0}\". Used with text \"{1}\".", token, context));
            }
        }

        public override string ToString()
        {
            var totalTextLength = _rootSnippet.GetLength();
            var sb = new StringBuilder(totalTextLength);
            _rootSnippet.ToString(sb);
            if (sb.Length != totalTextLength)
            {
                throw new InvalidOperationException(string.Format(
                    "Internal error: Calculated total text length ({0}) is different from actual ({1}).",
                    totalTextLength, sb.Length));
            }
            return sb.ToString();
        }

        #region Nested type: TokenOccurrence

        private sealed class TokenOccurrence
        {
            public int End; // Position of a token in the snippet.
            public FastReplacerSnippet Snippet;
            public int Start; // Position of a token in the snippet.
        }

        #endregion Nested type: TokenOccurrence
    }

    /// <summary>
    ///   </summary>
    /// <remarks>Taken here: http://www.codeproject.com/Articles/298519/Fast-Token-Replacement-in-Csharp</remarks>
    internal sealed class FastReplacerSnippet
    {
        public readonly string Text;
        private readonly List<InnerSnippet> _innerSnippets;

        public FastReplacerSnippet(string text)
        {
            Text = text;
            _innerSnippets = new List<InnerSnippet>();
        }

        public override string ToString()
        {
            return "Snippet: " + Text;
        }

        public void Append(FastReplacerSnippet snippet)
        {
            _innerSnippets.Add(new InnerSnippet
            {
                Snippet = snippet,
                Start = Text.Length,
                End = Text.Length,
                Order1 = 1,
                Order2 = _innerSnippets.Count
            });
        }

        public void Replace(int start, int end, FastReplacerSnippet snippet)
        {
            _innerSnippets.Add(new InnerSnippet
            {
                Snippet = snippet,
                Start = start,
                End = end,
                Order1 = 0,
                Order2 = 0
            });
        }

        public void InsertBefore(int start, FastReplacerSnippet snippet)
        {
            _innerSnippets.Add(new InnerSnippet
            {
                Snippet = snippet,
                Start = start,
                End = start,
                Order1 = 2,
                Order2 = _innerSnippets.Count
            });
        }

        public void InsertAfter(int end, FastReplacerSnippet snippet)
        {
            _innerSnippets.Add(new InnerSnippet
            {
                Snippet = snippet,
                Start = end,
                End = end,
                Order1 = 1,
                Order2 = _innerSnippets.Count
            });
        }

        public void ToString(StringBuilder sb)
        {
            _innerSnippets.Sort(delegate(InnerSnippet a, InnerSnippet b)
            {
                if (a == b)
                {
                    return 0;
                }
                if (a.Start != b.Start)
                {
                    return a.Start - b.Start;
                }
                if (a.End != b.End)
                {
                    return a.End - b.End;
                    // Disambiguation if there are inner snippets inserted before a token (they have
                    // End==Start) go before inner snippets inserted instead of a token (End>Start).
                }
                if (a.Order1 != b.Order1)
                {
                    return a.Order1 - b.Order1;
                }
                if (a.Order2 != b.Order2)
                {
                    return a.Order2 - b.Order2;
                }
                throw new InvalidOperationException(string.Format(
                    "Internal error: Two snippets have ambigous order. At position from {0} to {1}, order1 is {2}, order2 is {3}. First snippet is \"{4}\", second snippet is \"{5}\".",
                    a.Start, a.End, a.Order1, a.Order2, a.Snippet.Text, b.Snippet.Text));
            });
            var lastPosition = 0;
            foreach (var innerSnippet in _innerSnippets)
            {
                if (innerSnippet.Start < lastPosition)
                {
                    throw new InvalidOperationException(string.Format(
                        "Internal error: Token is overlapping with a previous token. Overlapping token is from position {0} to {1}, previous token ends at position {2} in snippet \"{3}\".",
                        innerSnippet.Start, innerSnippet.End, lastPosition, Text));
                }
                sb.Append(Text, lastPosition, innerSnippet.Start - lastPosition);
                innerSnippet.Snippet.ToString(sb);
                lastPosition = innerSnippet.End;
            }
            sb.Append(Text, lastPosition, Text.Length - lastPosition);
        }

        public int GetLength()
        {
            var len = Text.Length;
            foreach (var innerSnippet in _innerSnippets)
            {
                len -= innerSnippet.End - innerSnippet.Start;
                len += innerSnippet.Snippet.GetLength();
            }
            return len;
        }

        #region Nested type: InnerSnippet

        private sealed class InnerSnippet
        {
            public int End; // Position of the snippet in parent snippet's Text.
            public int Order1; // Order of snippets with a same Start position in their parent.
            public int Order2; // Order of snippets with a same Start position and Order1 in their parent.
            public FastReplacerSnippet Snippet;
            public int Start; // Position of the snippet in parent snippet's Text.

            public override string ToString()
            {
                return "InnerSnippet: " + Snippet.Text;
            }
        }

        #endregion Nested type: InnerSnippet
    }
}
