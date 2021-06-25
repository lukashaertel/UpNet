using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Lang
{
    /// <summary>
    /// OpenGL exception.
    /// </summary>
    [Serializable]
    public class GLException : Exception
    {
        /// <summary>
        /// Get current errors and throw if not <see cref="ErrorCode.NoError"/>.
        /// </summary>
        /// <exception cref="GLException">Throws the <see cref="GLException"/> if error state.</exception>
        public static void ThrowGlErrors()
        {
            // TODO: Loop through all errors.
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                throw new GLException($"Error performing the preceding methods, GL returned {error}", error);
        }

        /// <summary>
        /// Get current errors and throw if not <see cref="ErrorCode.NoError"/>.
        /// </summary>
        /// <param name="message">The message to use.</param>
        /// <exception cref="GLException">Throws the <see cref="GLException"/> if error state.</exception>
        public static void ThrowGlErrors(string? message)
        {
            // TODO: Format string maybe?
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                throw new GLException(message, error);
        }

        /// <summary>
        /// Run on entry.
        /// </summary>
        /// <param name="name">The name of the calling function.</param>
        public static void ThrowPreceding([CallerMemberName] string name = default!) =>
            ThrowGlErrors($"Errors were present before invoking {name}");

        /// <summary>
        /// The associated error code.
        /// </summary>
        public ErrorCode Code { get; }

        /// <summary>
        /// Creates the exception with the error code.
        /// </summary>
        /// <param name="code">The error code to use.</param>
        public GLException(ErrorCode code) =>
            Code = code;

        /// <summary>
        /// Creates the exception with the error code and the message.
        /// </summary>
        /// <param name="message">The message to use.</param>
        /// <param name="code">The error code to use.</param>
        public GLException(string? message, ErrorCode code) : base(message) =>
            Code = code;

        /// <summary>
        /// Creates the exception from serialization.
        /// </summary>
        /// <param name="info">The serialization info to read from.</param>
        /// <param name="context">The current streaming context.</param>
        protected GLException(SerializationInfo info, StreamingContext context) : base(info, context) =>
            Code = (ErrorCode) info.GetUInt16(nameof(Code));

        /// <inheritdoc/> 
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Code), (ushort) Code);
        }
    }
}