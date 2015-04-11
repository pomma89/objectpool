// File name: DataTableExtensions.cs
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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json;
using ErrorMessages = CodeProject.ObjectPool.Utilities.Core.ErrorMessages;

namespace CodeProject.ObjectPool.Utilities.Extensions
{
    /// <summary>
    ///   </summary>
    internal static class DataTableExtensions
    {
        /// <summary>
        ///   </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string ToJson(this DataTable dataTable)
        {
            Contract.Requires<ArgumentNullException>(dataTable != null, CodeProject.ObjectPool.Utilities.Core.ErrorMessages.Extensions_DataTableExtensions_NullDataTable);
            Contract.Ensures(Contract.Result<string>() != null);

            var columns = new GPair<DataColumn, string>[dataTable.Columns.Count];
            var idx = 0;
            foreach (DataColumn col in dataTable.Columns)
            {
                columns[idx++] = GPair.Create(col, col.ColumnName.Trim());
            }

            var rows = new Dictionary<string, object>[dataTable.Rows.Count];
            idx = 0;
            foreach (DataRow dr in dataTable.Rows)
            {
                rows[idx++] = columns.ToDictionary(col => col.Second, col => dr[col.First]);
            }

            return JsonConvert.SerializeObject(rows);
        }
    }
}

#endif