// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Shaders.Compiler
{
    /// <summary>
    /// Describes what kind of cache (if any) was used to retrieve the bytecode.
    /// </summary>
    public enum EffectBytecodeCacheLoadSource
    {
        /// <summary>
        /// The bytecode has just been compiled.
        /// </summary>
        JustCompiled = 0,

        /// <summary>
        /// The bytecode was loaded through the startup cache (part of asset compilation).
        /// </summary>
        StartupCache = 1,

        /// <summary>
        /// The bytecode was loaded through the runtime cache (not part of asset compilation).
        /// </summary>
        DynamicCache = 2,
    }
}
