using System;

namespace UpNet.Graphics.Graphics
{
    public abstract class Managed<T> : IDisposable where T : struct
    {
        /// <summary>
        /// Returns the GL object name
        /// </summary>
        public static implicit operator T(Managed<T> managed) => managed.Reference;

        /// <summary>
        /// True if the object was disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// The reference to the GL object.
        /// </summary>
        public T Reference { get; protected set; }

        protected virtual void DisposeContent()
        {
            // Default do nothing.
        }

        /// <summary>
        /// Disposes of the resource.
        /// </summary>
        public void Dispose()
        {
            // Ignore if already disposed.
            if (IsDisposed)
                return;

            // Do actual dispose.
            DisposeContent();

            // Set to unmanaged index.
            IsDisposed = true;
            Reference = default;
        }
    }
}