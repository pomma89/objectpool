// File name: StringBuilderPoolTests.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2013-2016 Alessio Parma <alessio.parma@gmail.com>
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
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using CodeProject.ObjectPool.Specialized;
using NLipsum.Core;
using NUnit.Framework;
using Shouldly;

namespace CodeProject.ObjectPool.UnitTests.Specialized
{
    [TestFixture]
    internal sealed class StringBuilderPoolTests
    {
        [SetUp]
        public void SetUp()
        {
            StringBuilderPool.Instance.Clear();
            StringBuilderPool.Instance.Diagnostics = new ObjectPoolDiagnostics
            {
                Enabled = true
            };
        }

        [TestCase("a", "b")]
        [TestCase("SNAU ORSO", "birretta")]
        [TestCase("PU <", "3 PI")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus.", "Aliquam scelerisque, SNAU.")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus.", "Aliquam scelerisque, lorem ac pretium luctus, nunc dui tincidunt sem, id rutrum nibh urna a neque. Maecenas lacus tellus, scelerisque nec faucibus ac, dignissim non justo. Vivamus volutpat at metus hendrerit feugiat. Donec imperdiet lobortis est a efficitur. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.")]
        public void ShouldReturnToPoolWhenStringIsSmall(string text1, string text2)
        {
            string result;
            using (var psb = StringBuilderPool.Instance.GetObject())
            {
                psb.StringBuilder.Append(text1);
                psb.StringBuilder.Append(text2);
                result = psb.StringBuilder.ToString();
            }

            result.ShouldBe(text1 + text2);

            StringBuilderPool.Instance.ObjectsInPoolCount.ShouldBe(1);
            StringBuilderPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(1);
            StringBuilderPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
        }

        [Test]
        public void ShouldNotReturnToPoolWhenStringIsLarge()
        {
            var text1 = LipsumGenerator.Generate(10);
            var text2 = LipsumGenerator.Generate(10);

            string result;
            using (var psb = StringBuilderPool.Instance.GetObject())
            {
                psb.StringBuilder.Append(text1);
                psb.StringBuilder.Append(text2);
                result = psb.StringBuilder.ToString();

                psb.StringBuilder.Capacity.ShouldBeGreaterThan(StringBuilderPool.MaximumStringBuilderCapacity);
            }

            result.ShouldBe(text1 + text2);

            StringBuilderPool.Instance.ObjectsInPoolCount.ShouldBe(0);
            StringBuilderPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(0);
            StringBuilderPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(1);
        }
    }
}