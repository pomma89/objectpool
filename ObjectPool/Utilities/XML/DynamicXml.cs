// File name: DynamicXml.cs
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

using Enumerable = System.Linq.Enumerable;

namespace CodeProject.ObjectPool.Utilities.Xml
{
    internal sealed class DynamicXml : System.Dynamic.DynamicObject
    {
        private const string ValueProperty = "Value";

        private readonly System.Collections.Generic.Dictionary<string, object> _propertyCache = new System.Collections.Generic.Dictionary<string, object>();
        private readonly System.Xml.Linq.XElement _root;

        private DynamicXml(System.Xml.Linq.XElement root)
        {
            _root = root;
        }

        /// <summary>
        ///   </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static DynamicXml Parse(string xmlString)
        {
            return new DynamicXml(System.Xml.Linq.XDocument.Parse(xmlString).Root);
        }

        /// <summary>
        ///   </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static DynamicXml Load(string uri)
        {
            return new DynamicXml(System.Xml.Linq.XDocument.Load(uri).Root);
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            return base.GetDynamicMemberNames();
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            var propertyName = binder.Name;

            if (_propertyCache.TryGetValue(propertyName, out result))
            {
                return true;
            }

            var attr = _root.Attribute(propertyName);
            if (attr != null)
            {
                result = attr.Value;
                return true;
            }

            var nodes = _root.Elements(propertyName);
            if (Enumerable.Count(nodes) > 1)
            {
                result = Enumerable.ToList(Enumerable.Select(nodes, n => new DynamicXml(n)));
                return true;
            }

            var node = _root.Element(propertyName);
            if (node != null)
            {
                if (node.HasElements)
                {
                    result = new DynamicXml(node);
                }
                else
                {
                    result = node.Value;
                }
                return true;
            }

            if (propertyName == ValueProperty)
            {
                result = _root.Value;
            }
            return true;
        }

        private bool Equals(DynamicXml other)
        {
            return _root.Equals(other._root);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is DynamicXml && Equals((DynamicXml) obj);
        }

        public override int GetHashCode()
        {
            return _root.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Root: [{0}]", _root);
        }
    }
}