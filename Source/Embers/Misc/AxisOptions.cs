using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Embers {
    public sealed class AxisOptions {
        /// <summary>
        /// If <see langword="true"/>, show <c>line:column</c>.<br/>
        /// If <see langword="false"/>, show <c>line</c>.<br/>
        /// Default: <see langword="false"/>.
        /// </summary>
        public bool ShowErrorColumn = false;
        /// <summary>
        /// The maximum number of mortal symbols before a random one is killed.<br/>
        /// Mortal symbols can be created with <c>string.to_sym</c>.<br/>
        /// Default: 5,000.
        /// </summary>
        public int MaxMortalSymbols = 5_000;
        /// <summary>
        /// If <see langword="true"/>, compile code where possible.<br/>
        /// If <see langword="false"/>, interpret all code.<br/>
        /// Default: <see langword="true"/>.
        /// </summary>
        public bool EnableCompilation = true;
        /// <summary>
        /// If <see langword="true"/>, use <see cref="ConcurrentDictionary{TKey, TValue}"/> and <see cref="SynchronizedCollection{T}"/> for thread safety.<br/>
        /// If <see langword="false"/>, use <see cref="Dictionary{TKey, TValue}"/> and <see cref="List{T}"/> for faster processing.<br/>
        /// Default: <see langword="false"/>.<br/>
        /// </summary>
        public bool ThreadSafety = false;
        /// <summary>
        /// If <see langword="true"/>, remove access to dangerous methods such as <c>File.read</c>.<br/>
        /// If <see langword="false"/>, allow access.<br/>
        /// Default: <see langword="false"/>.<br/>
        /// </summary>
        public bool Sandbox = false;
    }
}
