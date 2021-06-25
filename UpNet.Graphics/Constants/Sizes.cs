namespace UpNet.Graphics.Constants
{
    /// <summary>
    /// Shared size constants.
    /// </summary>
    public static class Sizes
    {
        /// <summary>
        /// Maximum number of shaders to retrieve on program linking.
        /// </summary>
        public const int AttachedShaderBufferSize = 16;

        /// <summary>
        /// Size to use when retrieving names of attribs and uniforms.
        /// </summary>
        public const int NameBufferSize = 64;

        /// <summary>
        /// Maximum number of bytes to allocate on the stack.
        /// </summary>
        public const int StackallocThreshold = 2048;
    }
}