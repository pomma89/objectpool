// File name: Environment.cs
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

#if !PORTABLE

using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace CodeProject.ObjectPool
{
    /// <summary>
    ///   TODO
    /// </summary>
    internal static class GEnvironment
    {
        public static bool AppIsRunningOnAspNet
        {
            get { return "web.config".Equals(Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile), StringComparison.OrdinalIgnoreCase); }
        }

        // Order is important!
        private static readonly string[] MapPathStarts = { "~//", "~\\\\", "~/", "~\\", "~" };

        public static string MapPath(this string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(Contract.Result<string>().Length > 0);

            if (Path.IsPathRooted(path))
            {
                return path;
            }
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var trimmedPath = path.Trim();
            foreach (var start in MapPathStarts)
            {
                if (trimmedPath.StartsWith(start))
                {
                    trimmedPath = trimmedPath.Substring(start.Length, trimmedPath.Length - start.Length);
                    break;
                }
            }
            return Path.Combine(basePath, trimmedPath);
        }
    }
}

#endif
