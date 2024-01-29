using System;
using System.Collections;

namespace Embers {
    /// <summary>A splat (*) argument that takes multiple arguments and passes an array.</summary>
    /// <remarks>The argument must implement <see cref="IList"/>.</remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class SplatAttribute : Attribute {
    }
    /// <summary>A double splat (**) argument that takes multiple key-value arguments and passes a hash.</summary>
    /// <remarks>The argument must be a <see cref="Hash"/>.</remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DoubleSplatAttribute : Attribute {
    }
    /// <summary>A block (&amp;) argument that takes a block and passes a proc.</summary>
    /// <remarks>The argument must be a <see cref="Proc"/>?.</remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class BlockAttribute : Attribute {
    }
}
