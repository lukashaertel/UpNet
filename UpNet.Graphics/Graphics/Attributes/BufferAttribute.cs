using System;

namespace UpNet.Graphics.Graphics.Attributes
{
    /// <summary>
    /// Marks a field or property as the target of buffer generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class BufferAttribute : Attribute
    {
    }
}