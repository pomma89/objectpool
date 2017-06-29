// Copyright (c) GZNB. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace CodeProject.ObjectPool
{
    /// <summary>
    /// Eviction Config Infomation
    /// </summary>
    public class EvictionConfig
    {
        /// <summary>
        /// Eviction Is Enable default is false
        /// </summary>
        public bool Enable { get; set; } = false;

        /// <summary>
        /// Timer delay time default is zero
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Timer period default is one minute
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromMinutes(1);
    }
}