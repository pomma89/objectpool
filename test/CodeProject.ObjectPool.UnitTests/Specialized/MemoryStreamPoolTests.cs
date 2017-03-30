// File name: MemoryStreamPoolTests.cs
//
// Author(s): Alessio Parma <alessio.parma@gmail.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2013-2018 Alessio Parma <alessio.parma@gmail.com>
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

using CodeProject.ObjectPool.Core;
using CodeProject.ObjectPool.Specialized;
using Newtonsoft.Json;
using NLipsum.Core;
using NUnit.Framework;
using Shouldly;
using System;
using System.IO;
using System.Text;

namespace CodeProject.ObjectPool.UnitTests.Specialized
{
    [TestFixture]
    internal sealed class MemoryStreamPoolTests
    {
        private IMemoryStreamPool _memoryStreamPool;

        [SetUp]
        public void SetUp()
        {
            _memoryStreamPool = MemoryStreamPool.Instance;
            _memoryStreamPool.Clear();
            _memoryStreamPool.Diagnostics = new ObjectPoolDiagnostics
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
            using (var pms = _memoryStreamPool.GetObject())
            {
#pragma warning disable CC0022 // Should dispose object
                var sw = new StreamWriter(pms.MemoryStream);
                sw.Write(text);
                sw.Flush();

                pms.MemoryStream.Position = 0L;

                var sr = new StreamReader(pms.MemoryStream);
                result = sr.ReadToEnd();
#pragma warning restore CC0022 // Should dispose object

                pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(_memoryStreamPool.MaximumMemoryStreamCapacity);
            }

            result.ShouldBe(text);

            _memoryStreamPool.ObjectsInPoolCount.ShouldBe(1);
            _memoryStreamPool.Diagnostics.ReturnedToPoolCount.ShouldBe(1);
            _memoryStreamPool.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
        }

        [TestCase("a&b")]
        [TestCase("SNAU ORSO birretta")]
        [TestCase("PU <3 PI")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. --> Aliquam scelerisque, SNAU.")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eget ante risus. In rhoncus mattis leo, in tincidunt felis euismod sed. Pellentesque rhoncus elementum lacus tincidunt feugiat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Aliquam scelerisque, lorem ac pretium luctus, nunc dui tincidunt sem, id rutrum nibh urna a neque. Maecenas lacus tellus, scelerisque nec faucibus ac, dignissim non justo. Vivamus volutpat at metus hendrerit feugiat. Donec imperdiet lobortis est a efficitur. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.")]
        public void ShouldReturnToPoolWhenStreamIsSmall_TwoTimes(string text)
        {
            string result;
            using (var pms = _memoryStreamPool.GetObject())
            {
#pragma warning disable CC0022 // Should dispose object
                var sw = new StreamWriter(pms.MemoryStream);
                sw.Write(text);
                sw.Flush();

                pms.MemoryStream.Position = 0L;

                var sr = new StreamReader(pms.MemoryStream);
                result = sr.ReadToEnd();
#pragma warning restore CC0022 // Should dispose object

                pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(_memoryStreamPool.MaximumMemoryStreamCapacity);
            }
            result.ShouldBe(text);

            using (var pms = _memoryStreamPool.GetObject())
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

                pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(_memoryStreamPool.MaximumMemoryStreamCapacity);
            }
            result.ShouldBe(text + text);

            _memoryStreamPool.ObjectsInPoolCount.ShouldBe(1);
            _memoryStreamPool.Diagnostics.ReturnedToPoolCount.ShouldBe(2);
            _memoryStreamPool.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
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
            var pms = _memoryStreamPool.GetObject();

            var sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            var sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(_memoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
#pragma warning restore CC0022 // Should dispose object

            result.ShouldBe(text);

            _memoryStreamPool.ObjectsInPoolCount.ShouldBe(1);
            _memoryStreamPool.Diagnostics.ReturnedToPoolCount.ShouldBe(1);
            _memoryStreamPool.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
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
            var pms = _memoryStreamPool.GetObject();
            var sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            var sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(_memoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
            result.ShouldBe(text);

            // Second
            pms = _memoryStreamPool.GetObject();
            sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeLessThanOrEqualTo(_memoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
            result.ShouldBe(text + text);
#pragma warning restore CC0022 // Should dispose object

            _memoryStreamPool.ObjectsInPoolCount.ShouldBe(1);
            _memoryStreamPool.Diagnostics.ReturnedToPoolCount.ShouldBe(2);
            _memoryStreamPool.Diagnostics.ObjectResetFailedCount.ShouldBe(0);
        }

        [Test]
        public void ShouldNotReturnToPoolWhenStreamIsLarge()
        {
            var text = LipsumGenerator.Generate(1000);

            string result;
            using (var pms = _memoryStreamPool.GetObject())
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

                pms.MemoryStream.Capacity.ShouldBeGreaterThan(_memoryStreamPool.MaximumMemoryStreamCapacity);
            }

            result.ShouldBe(text + text);

            _memoryStreamPool.ObjectsInPoolCount.ShouldBe(0);
            _memoryStreamPool.Diagnostics.ReturnedToPoolCount.ShouldBe(0);
            _memoryStreamPool.Diagnostics.ObjectResetFailedCount.ShouldBe(1);
        }

        [Test]
        public void ShouldNotReturnToPoolWhenStreamIsLargeAndStreamIsManuallyDisposed()
        {
            var text = LipsumGenerator.Generate(1000);

            string result;
#pragma warning disable CC0022 // Should dispose object
            var pms = _memoryStreamPool.GetObject();

            var sw = new StreamWriter(pms.MemoryStream);
            sw.Write(text);
            sw.Write(text);
            sw.Flush();

            pms.MemoryStream.Position = 0L;

            var sr = new StreamReader(pms.MemoryStream);
            result = sr.ReadToEnd();

            pms.MemoryStream.Capacity.ShouldBeGreaterThan(_memoryStreamPool.MaximumMemoryStreamCapacity);
            pms.MemoryStream.Dispose();
#pragma warning restore CC0022 // Should dispose object

            result.ShouldBe(text + text);

            _memoryStreamPool.ObjectsInPoolCount.ShouldBe(0);
            _memoryStreamPool.Diagnostics.ReturnedToPoolCount.ShouldBe(0);
            _memoryStreamPool.Diagnostics.ObjectResetFailedCount.ShouldBe(1);
        }

        [Test]
        public void IdPropertyShouldNotChangeUsageAfterUsage()
        {
            // First usage.
            int id;
            using (var pms = _memoryStreamPool.GetObject())
            {
                id = pms.PooledObjectInfo.Id;
                id.ShouldNotBe(0);
            }

            // Second usage is the same, pool uses a sort of stack, not a proper queue.
            using (var pms = _memoryStreamPool.GetObject())
            {
                pms.PooledObjectInfo.Id.ShouldBe(id);
            }
        }

        [Test]
        public void ShouldNotClearPoolWhenMinCapacityIsDecreased()
        {
            int initialCapacity;
            using (var pms = _memoryStreamPool.GetObject())
            {
                initialCapacity = pms.MemoryStream.Capacity;
            }

            initialCapacity.ShouldBe(_memoryStreamPool.MinimumMemoryStreamCapacity);

            _memoryStreamPool.MinimumMemoryStreamCapacity = initialCapacity / 2;
            using (var pms = _memoryStreamPool.GetObject())
            {
                pms.MemoryStream.Capacity.ShouldBe(initialCapacity);
            }
        }

        [Test]
        public void ShouldClearPoolWhenMinCapacityIsIncreased()
        {
            int initialCapacity;
            using (var pms = _memoryStreamPool.GetObject())
            {
                initialCapacity = pms.MemoryStream.Capacity;
            }

            initialCapacity.ShouldBe(_memoryStreamPool.MinimumMemoryStreamCapacity);

            _memoryStreamPool.MinimumMemoryStreamCapacity = initialCapacity * 2;
            using (var pms = _memoryStreamPool.GetObject())
            {
                pms.MemoryStream.Capacity.ShouldBe(_memoryStreamPool.MinimumMemoryStreamCapacity);
            }
        }

        [Test]
        public void ShouldNotClearPoolWhenMaxCapacityIsIncreased()
        {
            int initialId;
            using (var pms = _memoryStreamPool.GetObject())
            {
                initialId = pms.PooledObjectInfo.Id;
            }

            initialId.ShouldBeGreaterThan(0);

            _memoryStreamPool.MaximumMemoryStreamCapacity *= 2;
            using (var pms = _memoryStreamPool.GetObject())
            {
                pms.PooledObjectInfo.Id.ShouldBe(initialId);
            }
        }

        [Test]
        public void ShouldClearPoolWhenMaxCapacityIsDecreased()
        {
            int initialId;
            using (var pms = _memoryStreamPool.GetObject())
            {
                initialId = pms.PooledObjectInfo.Id;
            }

            initialId.ShouldBeGreaterThan(0);

            _memoryStreamPool.MaximumMemoryStreamCapacity /= 2;
            using (var pms = _memoryStreamPool.GetObject())
            {
                pms.PooledObjectInfo.Id.ShouldBeGreaterThan(initialId);
            }
        }

        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void ShouldAllowCommonUsingPattern_ManyTimesWithStringReaderAndWriter(int count)
        {
            for (var i = 1; i <= count; ++i)
            {
                using (var ms = _memoryStreamPool.GetObject().MemoryStream)
                using (var sw = new StreamWriter(ms))
                {
                    var text = LipsumGenerator.Generate((i % 10) + 1);

                    sw.Write(text);
                    sw.Flush();

                    ms.Position = 0L;
                    using (var sr = new StreamReader(ms))
                    {
                        sr.ReadToEnd().ShouldBe(text);
                    }
                }
            }
        }

        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void ShouldAllowCommonUsingPattern_ManyTimesWithJsonReaderAndWriter(int count)
        {
            var jsonSerializer = new JsonSerializer();

            for (var i = 1; i <= count; ++i)
            {
                using (var ms = _memoryStreamPool.GetObject().MemoryStream)
                using (var sw = new StreamWriter(ms))
                using (var jw = new JsonTextWriter(sw))
                {
                    var text = LipsumGenerator.Generate((i % 10) + 1);

                    jsonSerializer.Serialize(jw, text);
                    jw.Flush();

                    ms.Position = 0L;
                    using (var sr = new StreamReader(ms))
                    using (var jr = new JsonTextReader(sr))
                    {
                        jsonSerializer.Deserialize<string>(jr).ShouldBe(text);
                    }
                }
            }
        }
    }
}