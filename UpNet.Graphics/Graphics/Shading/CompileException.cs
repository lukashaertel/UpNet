using System;
using System.Runtime.Serialization;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Graphics.Shading
{
    /// <summary>
    /// Program compile exception.
    /// </summary>
    [Serializable]
    public class CompileException : Exception
    {
        /// <summary>
        /// The output of the compiler.
        /// </summary>
        public string Info { get; }

        /// <summary>
        /// The type of the shader.
        /// </summary>
        public ShaderType Type { get; }

        /// <summary>
        /// The source of that compilation.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Creates the exception with the compiler output.
        /// </summary>
        /// <param name="output">The compiler output.</param>
        /// <param name="type">The type of the shader.</param>
        /// <param name="body">The source of that compilation.</param>
        public CompileException(string output, ShaderType type, string body) =>
            (Info, Type, Body) = (output, type, body);

        /// <summary>
        /// Creates the exception with the compiler output and the message.
        /// </summary>
        /// <param name="message">The message to use.</param>
        /// <param name="info">The compiler output.</param>
        /// <param name="type">The type of the shader.</param>
        /// <param name="body">The source of that compilation.</param>
        public CompileException(string? message, string info, ShaderType type, string body) : base(message) =>
            (Info, Type, Body) = (info, type, body);

        /// <summary>
        /// Creates the exception from serialization.
        /// </summary>
        /// <param name="info">The serialization info to read from.</param>
        /// <param name="context">The current streaming context.</param>
        protected CompileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Info = info.GetString(nameof(Info))
                   ?? throw new SerializationException($"{nameof(Info)} should not have been null");
            Type = (ShaderType) info.GetUInt32(nameof(Type));
            Body = info.GetString(nameof(Body))
                   ?? throw new SerializationException($"{nameof(Body)} should not have been null");
        }

        /// <inheritdoc/> 
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Info), Info);
            info.AddValue(nameof(Type), (uint) Type);
            info.AddValue(nameof(Body), Body);
        }
    }
}