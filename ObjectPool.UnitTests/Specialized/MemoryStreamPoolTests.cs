// File name: MemoryStreamPoolTests.cs
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
using System.IO;
using System.Text;

namespace CodeProject.ObjectPool.UnitTests.Specialized
{
    [TestFixture]
    internal sealed class MemoryStreamPoolTests
    {
        [SetUp]
        public void SetUp()
        {
            MemoryStreamPool.Instance.Clear();
            MemoryStreamPool.Instance.Diagnostics = new ObjectPoolDiagnostics
            {
                Enabled = true
            };
        }

        [TestCase("a&b")]
        [TestCase("SNAU ORSO birretta")]
        [TestCase("PU <3 PI")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. --> Aliquam scelerisque, SNAU.")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Aliquam scelerisque, lorem ac pretium luctus, nunc dui tincidunt sem, id rutrum nibh urna a neque. Maecenas lacus tellus, scelerisque nec faucibus ac, dignissim non justo. Vivamus volutpat at metus hendrerit feugiat. Donec imperdiet lobortis est a efficitur. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.")]
        public void ShouldReturnToPoolWhenStreamIsSmall(string text)
        {
            string result;
            using (var pms = MemoryStreamPool.Instance.GetObject())
            {
#pragma warning disable CC0022 // Should dispose object
                var sw = new StreamWriter(pms.MemoryStream);
                sw.Write(text);
                sw.Flush();

                pms.MemoryStream.Position = 0L;

                var sr = new StreamReader(pms.MemoryStream);
                result = sr.ReadToEnd();
#pragma warning restore CC0022 // Should dispose object

                pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(MemoryStreamPool.MaximumMemoryStreamCapacity);
            }

            result.ShouldBe(text);

            MemoryStreamPool.Instance.ObjectsInPoolCount.ShouldBe(1);
            MemoryStreamPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(1);
            MemoryStreamPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
        }

        [TestCase("a&b")]
        [TestCase("SNAU ORSO birretta")]
        [TestCase("PU <3 PI")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. --> Aliquam scelerisque, SNAU.")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Aliquam scelerisque, lorem ac pretium luctus, nunc dui tincidunt sem, id rutrum nibh urna a neque. Maecenas lacus tellus, scelerisque nec faucibus ac, dignissim non justo. Vivamus volutpat at metus hendrerit feugiat. Donec imperdiet lobortis est a efficitur. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.")]
        public void ShouldReturnToPoolWhenStreamIsSmall_TwoTimes(string text)
        {
            string result;
            using (var pms = MemoryStreamPool.Instance.GetObject())
            {
#pragma warning disable CC0022 // Should dispose object
                var sw = new StreamWriter(pms.MemoryStream);
                sw.Write(text);
                sw.Flush();

                pms.MemoryStream.Position = 0L;

                var sr = new StreamReader(pms.MemoryStream);
                result = sr.ReadToEnd();
#pragma warning restore CC0022 // Should dispose object

                pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(MemoryStreamPool.MaximumMemoryStreamCapacity);
            }
            result.ShouldBe(text);

            using (var pms = MemoryStreamPool.Instance.GetObject())
            {
#pragma warning disable CC0022 // Should dispose object
                var sw = new StreamWriter(pms.MemoryStream);
                sw.Write(text);
                sw.Write(text);
                sw.Flush();

                pms.MemoryStream.Position = 0L;

                var sr = new StreamReader(pms.MemoryStream);
                result = sr.ReadToEnd();
#pragma warning restore CC0022 // Should dispose object

                pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(MemoryStreamPool.MaximumMemoryStreamCapacity);
            }
            result.ShouldBe(text + text);

            MemoryStreamPool.Instance.ObjectsInPoolCount.ShouldBe(1);
            MemoryStreamPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(2);
            MemoryStreamPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
        }

        [TestCase("a&b")]
        [TestCase("SNAU ORSO birretta")]
        [TestCase("PU <3 PI")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. --> Aliquam scelerisque, SNAU.")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Aliquam scelerisque, lorem ac pretium luctus, nunc dui tincidunt sem, id rutrum nibh urna a neque. Maecenas lacus tellus, scelerisque nec faucibus ac, dignissim non justo. Vivamus volutpat at metus hendrerit feugiat. Donec imperdiet lobortis est a efficitur. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.")]
        public void ShouldReturnToPoolWhenStreamIsSmallAndStreamIsManuallyDisposed(string text)
        {
            string result;
#pragma warning disable CC0022 // Should dispose object
            var pms = MemoryStreamPool.Instance.GetObject();

            var sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            var sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(MemoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
#pragma warning restore CC0022 // Should dispose object

            result.ShouldBe(text);

            MemoryStreamPool.Instance.ObjectsInPoolCount.ShouldBe(1);
            MemoryStreamPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(1);
            MemoryStreamPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
        }

        [TestCase("a&b")]
        [TestCase("SNAU ORSO birretta")]
        [TestCase("PU <3 PI")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. --> Aliquam scelerisque, SNAU.")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Aliquam scelerisque, lorem ac pretium luctus, nunc dui tincidunt sem, id rutrum nibh urna a neque. Maecenas lacus tellus, scelerisque nec faucibus ac, dignissim non justo. Vivamus volutpat at metus hendrerit feugiat. Donec imperdiet lobortis est a efficitur. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.")]
        public void ShouldReturnToPoolWhenStreamIsSmallAndStreamIsManuallyDisposed_TwoTimes(string text)
        {
            string result;
#pragma warning disable CC0022 // Should dispose object
            // First
            var pms = MemoryStreamPool.Instance.GetObject();
            var sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            var sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(MemoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
            result.ShouldBe(text);

            // Second
            pms = MemoryStreamPool.Instance.GetObject();
            sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(MemoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
            result.ShouldBe(text + text);
#pragma warning restore CC0022 // Should dispose object

            MemoryStreamPool.Instance.ObjectsInPoolCount.ShouldBe(1);
            MemoryStreamPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(2);
            MemoryStreamPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
        }

        [Test]
        public void ShouldNotReturnToPoolWhenStreamIsLarge()
        {
            var text = LipsumGenerator.Generate(1000);

            string result;
            using (var pms = MemoryStreamPool.Instance.GetObject())
            {
#pragma warning disable CC0022 // Should dispose object
                var sw = new StreamWriter(pms.MemoryStream);
                sw.Write(text);
                sw.Write(text);
                sw.Flush();

                pms.MemoryStream.Position = 0L;

                var sr = new StreamReader(pms.MemoryStream);
                result = sr.ReadToEnd();
#pragma warning restore CC0022 // Should dispose object

                pms.MemoryStream.Capacity.ShouldBeGreaterThan(MemoryStreamPool.MaximumMemoryStreamCapacity);
            }

            result.ShouldBe(text + text);

            MemoryStreamPool.Instance.ObjectsInPoolCount.ShouldBe(0);
            MemoryStreamPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(0);
            MemoryStreamPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(1);
        }

        [Test]
        public void ShouldNotReturnToPoolWhenStreamIsLargeAndStreamIsManuallyDisposed()
        {
            var text = LipsumGenerator.Generate(1000);

            string result;
#pragma warning disable CC0022 // Should dispose object
            var pms = MemoryStreamPool.Instance.GetObject();

            var sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            var sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeGreaterThan(MemoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
#pragma warning restore CC0022 // Should dispose object

            result.ShouldBe(text + text);

            MemoryStreamPool.Instance.ObjectsInPoolCount.ShouldBe(0);
            MemoryStreamPool.Instance.Diagnostics.ReturnedToPoolCount.ShouldBe(0);
            MemoryStreamPool.Instance.Diagnostics.ObjectResetFailedCount.ShouldBe(1);
        }
    }
}