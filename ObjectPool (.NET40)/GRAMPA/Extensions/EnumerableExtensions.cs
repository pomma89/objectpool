// File name: EnumerableExtensions.cs
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

using System.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

#endif

using System.Collections.Generic;

namespace CodeProject.ObjectPool.Extensions
{
    internal static partial class EnumerableExtensions
    {
        #region ToHashSet

        /// <summary>
        ///   Converts given enumerable into an <see cref="HashSet{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The elements that should be added to the set.</param>
        /// <returns>An <see cref="HashSet{T}"/> filled from <paramref name="source"/>.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        /// <summary>
        ///   Converts given enumerable into an <see cref="HashSet{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The elements that should be added to the set.</param>
        /// <param name="equalityComparer">The equality comparer that will be used by the set.</param>
        /// <returns>An <see cref="HashSet{T}"/> filled from <paramref name="source"/>.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> equalityComparer)
        {
            return new HashSet<T>(source, equalityComparer);
        }

        #endregion ToHashSet
    }
}

#if !PORTABLE

namespace CodeProject.ObjectPool.Extensions
{
    internal static partial class EnumerableExtensions
    {
        /// <summary>
        ///   </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable)
        {
            // Create DataTable Structure
            PropertyDescriptorCollection properties;
            var table = CreateTable<T>(out properties);
            // Get the list item and add into the list
            foreach (var item in enumerable)
            {
                var row = table.NewRow();
                foreach (PropertyDescriptor property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        ///   </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IEnumerable<dynamic> enumerable)
        {
            // Create DataTable Structure
            var first = true;
            var table = new DataTable();
            Dictionary<string, ColumnInfo> columns = null;
            // Get the list item and add into the list
            foreach (var item in enumerable.Cast<IDictionary<string, object>>())
            {
                if (first)
                {
                    table = CreateTable(item, out columns);
                    first = false;
                }
                Debug.Assert(columns != null && table != null);
                var row = table.NewRow();
                foreach (var column in columns)
                {
                    var propertyValue = item[column.Key];
                    if (!column.Value.TypeIsSet && propertyValue != null)
                    {
                        var propertyType = propertyValue.GetType();
                        var columnType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                        ChangeColumnType(table, column.Value.Index, columnType);
                        column.Value.TypeIsSet = true;
                    }
                    row[column.Value.Index] = propertyValue;
                }
                table.Rows.Add(row);
            }
            return table;
        }

#region Private Methods

        private static DataTable CreateTable<T>(out PropertyDescriptorCollection properties)
        {
            // T –> ClassName
            var entityType = typeof(T);
            // Set the datatable name as class name
            var table = new DataTable(entityType.Name);
            // Get the property list
            properties = TypeDescriptor.GetProperties(entityType);
            foreach (PropertyDescriptor property in properties)
            {
                // Add property as column
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                table.Columns.Add(property.Name, type);
            }
            return table;
        }

        private static DataTable CreateTable(IDictionary<string, object> item, out Dictionary<string, ColumnInfo> columns)
        {
            var index = 0;
            var table = new DataTable();
            columns = new Dictionary<string, ColumnInfo>();
            foreach (var propertyName in item.Keys)
            {
                // Add property as column
                var propertyValue = item[propertyName];
                if (propertyValue != null)
                {
                    var propertyType = propertyValue.GetType();
                    var columnType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                    table.Columns.Add(propertyName, columnType);
                    columns.Add(propertyName, new ColumnInfo { Index = index++, TypeIsSet = false });
                }
                else
                {
                    table.Columns.Add(propertyName);
                    columns.Add(propertyName, new ColumnInfo { Index = index++, TypeIsSet = false });
                }
            }
            return table;
        }

        private static void ChangeColumnType(DataTable dataTable, int columnIndex, Type newColumnType)
        {
            var oldColumn = dataTable.Columns[columnIndex];
            if (oldColumn.DataType == newColumnType)
            {
                return;
            }
            var newColumn = new DataColumn("$temporary^", newColumnType);
            dataTable.Columns.Add(newColumn);
            newColumn.SetOrdinal(columnIndex); // To make sure column is inserted at the same place.
            foreach (DataRow row in dataTable.Rows)
            {
                var oldValue = row[columnIndex + 1];
                if (!(oldValue is DBNull))
                {
                    row[columnIndex] = Convert.ChangeType(oldValue, newColumnType);
                }
            }
            dataTable.Columns.Remove(oldColumn.ColumnName);
            newColumn.ColumnName = oldColumn.ColumnName;
        }

        private sealed class ColumnInfo
        {
            public int Index { get; set; }

            public bool TypeIsSet { get; set; }
        }

#endregion Private Methods
    }
}

#endif
