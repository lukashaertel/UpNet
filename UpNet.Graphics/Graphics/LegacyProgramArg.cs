using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Graphics
{
    /// <summary>
    /// An argument to <see cref="LegacyProgram"/>.
    /// </summary>
    [Obsolete]
    public readonly struct LegacyProgramArg
    {
        /// <summary>
        /// Extension creating a <see cref="ShaderType.ComputeShader"/> when using <see cref="FromPathsAsync"/>.
        /// </summary>
        public static readonly string ExtensionCompute = ".comp";

        /// <summary>
        /// Extension creating a <see cref="ShaderType.FragmentShader"/> when using <see cref="FromPathsAsync"/>.
        /// </summary>
        public static readonly string ExtensionFragment = ".frag";

        /// <summary>
        /// Extension creating a <see cref="ShaderType.VertexShader"/> when using <see cref="FromPathsAsync"/>.
        /// </summary>
        public static readonly string ExtensionVertex = ".vert";

        /// <summary>
        /// Extension creating a <see cref="ShaderType.GeometryShader"/> when using <see cref="FromPathsAsync"/>.
        /// </summary>
        public static readonly string ExtensionGeometry = ".geom";

        /// <summary>
        /// Extension creating a <see cref="ShaderType.TessEvaluationShader"/> when using <see cref="FromPathsAsync"/>.
        /// </summary>
        public static readonly string ExtensionTessEvaluation = ".tees";

        /// <summary>
        /// Extension creating a <see cref="ShaderType.TessControlShader"/> when using <see cref="FromPathsAsync"/>.
        /// </summary>
        public static readonly string ExtensionTessControl = ".tecs";

        /// <summary>
        /// Reads the content of the <paramref name="paths"/> and uses the file extensions to determine their type.
        /// </summary>
        /// <param name="paths">The input paths.</param>
        /// <returns>Returns an array of <see cref="LegacyProgramArg"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when an extension cannot be mapped.</exception>
        public static async Task<LegacyProgramArg[]> FromPathsAsync(params string[] paths)
        {
            // Make parameter array.
            var args = new LegacyProgramArg[paths.Length];

            // Read all files.
            var data = await Task.WhenAll(paths.Select(path => File.ReadAllTextAsync(path)));

            // Fill parameter array from paths.
            for (var i = 0; i < paths.Length; i++)
            {
                // Get the path and it's extension.
                var path = paths[i];
                var extension = Path.GetExtension(path);

                // Match with known names or throw.
                if (extension.Equals(ExtensionCompute, StringComparison.InvariantCultureIgnoreCase))
                    args[i] = new LegacyProgramArg(ShaderType.ComputeShader, data[i]);
                else if (extension.Equals(ExtensionFragment, StringComparison.InvariantCultureIgnoreCase))
                    args[i] = new LegacyProgramArg(ShaderType.FragmentShader, data[i]);
                else if (extension.Equals(ExtensionVertex, StringComparison.InvariantCultureIgnoreCase))
                    args[i] = new LegacyProgramArg(ShaderType.VertexShader, data[i]);
                else if (extension.Equals(ExtensionGeometry, StringComparison.InvariantCultureIgnoreCase))
                    args[i] = new LegacyProgramArg(ShaderType.GeometryShader, data[i]);
                else if (extension.Equals(ExtensionTessEvaluation, StringComparison.InvariantCultureIgnoreCase))
                    args[i] = new LegacyProgramArg(ShaderType.TessEvaluationShader, data[i]);
                else if (extension.Equals(ExtensionTessControl, StringComparison.InvariantCultureIgnoreCase))
                    args[i] = new LegacyProgramArg(ShaderType.TessControlShader, data[i]);
                else
                    throw new ArgumentException($"The extension {extension} is not mapped", nameof(paths));
            }

            // Return created arguments.
            return args;
        }

        /// <summary>
        /// <para>
        /// Reads the content of the <paramref name="paths"/> and uses the file extensions to determine their type.
        /// </para>
        /// <para>
        /// Shorthand notation for <code><![CDATA[FromPathsAsync(paths).GetAwaiter().GetResult()]]></code>
        /// </para>
        /// </summary>
        /// <param name="paths">The input paths.</param>
        /// <returns>Returns an array of <see cref="LegacyProgramArg"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when an extension cannot be mapped.</exception>
        public static LegacyProgramArg[] FromPaths(params string[] paths) =>
            FromPathsAsync(paths).GetAwaiter().GetResult();

        /// <summary>
        /// The type of shader to set.
        /// </summary>
        public ShaderType Type { get; }

        /// <summary>
        /// The source of the shader.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Initializes the argument.
        /// </summary>
        /// <param name="type">The type of shader to set.</param>
        /// <param name="source">The source of the shader.</param>
        public LegacyProgramArg(ShaderType type, string source) =>
            (Type, Source) = (type, source);
    }
}