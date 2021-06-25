using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using UpNet.Graphics.Lang;

namespace UpNet.Graphics.Graphics.Shading
{
    /// <summary>
    /// Extends <see cref="Uniform"/>.
    /// </summary>
    public static class UniformExtensions
    {
        /// <summary>
        /// True if the <see cref="Uniform"/> is a sampler.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if a sampler type.</returns>
        public static bool IsSampler(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.Sampler1d => true,
                UniformType.Sampler2d => true,
                UniformType.Sampler3d => true,
                UniformType.SamplerCube => true,
                UniformType.Sampler1dShadow => true,
                UniformType.Sampler2dShadow => true,
                UniformType.Sampler1dArray => true,
                UniformType.Sampler2dArray => true,
                UniformType.Sampler1dArrayShadow => true,
                UniformType.Sampler2dArrayShadow => true,
                UniformType.SamplerCubeShadow => true,
                UniformType.IntSampler1d => true,
                UniformType.IntSampler2d => true,
                UniformType.IntSampler3d => true,
                UniformType.IntSamplerCube => true,
                UniformType.IntSampler1dArray => true,
                UniformType.IntSampler2dArray => true,
                UniformType.UnsignedIntSampler1d => true,
                UniformType.UnsignedIntSampler2d => true,
                UniformType.UnsignedIntSampler3d => true,
                UniformType.UnsignedIntSamplerCube => true,
                UniformType.UnsignedIntSampler1dArray => true,
                UniformType.UnsignedIntSampler2dArray => true,
                UniformType.Sampler2dRect => true,
                UniformType.Sampler2dRectShadow => true,
                UniformType.SamplerBuffer => true,
                UniformType.IntSampler2dRect => true,
                UniformType.IntSamplerBuffer => true,
                UniformType.UnsignedIntSampler2dRect => true,
                UniformType.UnsignedIntSamplerBuffer => true,
                UniformType.Sampler2dMultisample => true,
                UniformType.IntSampler2dMultisample => true,
                UniformType.UnsignedIntSampler2dMultisample => true,
                UniformType.Sampler2dMultisampleArray => true,
                UniformType.IntSampler2dMultisampleArray => true,
                UniformType.UnsignedIntSampler2dMultisampleArray => true,
                UniformType.SamplerCubeMapArray => true,
                UniformType.SamplerCubeMapArrayShadow => true,
                UniformType.IntSamplerCubeMapArray => true,
                UniformType.UnsignedIntSamplerCubeMapArray => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is a matrix.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if a matrix type.</returns>
        public static bool IsMatrix(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.FloatMat2 => true,
                UniformType.FloatMat3 => true,
                UniformType.FloatMat4 => true,
                UniformType.FloatMat2x3 => true,
                UniformType.FloatMat2x4 => true,
                UniformType.FloatMat3x2 => true,
                UniformType.FloatMat3x4 => true,
                UniformType.FloatMat4x2 => true,
                UniformType.FloatMat4x3 => true,
                UniformType.DoubleMat2 => true,
                UniformType.DoubleMat3 => true,
                UniformType.DoubleMat4 => true,
                UniformType.DoubleMat2x3 => true,
                UniformType.DoubleMat2x4 => true,
                UniformType.DoubleMat3x2 => true,
                UniformType.DoubleMat3x4 => true,
                UniformType.DoubleMat4x2 => true,
                UniformType.DoubleMat4x3 => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is a matrix of the given dimension.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <param name="dimensions">The dimensions to check.</param>
        /// <returns>True if a matrix type.</returns>
        public static bool IsMatrix(this Uniform uniform, int dimensions) =>
            (dimensions, uniform.Type) switch
            {
                (2, UniformType.FloatMat2) => true,
                (3, UniformType.FloatMat3) => true,
                (4, UniformType.FloatMat4) => true,
                (2, UniformType.DoubleMat2) => true,
                (3, UniformType.DoubleMat3) => true,
                (4, UniformType.DoubleMat4) => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is a matrix of the given dimension.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <param name="columns">The columns to check.</param>
        /// <param name="rows">The rows to check.</param>
        /// <returns>True if a matrix type.</returns>
        public static bool IsMatrix(this Uniform uniform, int columns, int rows) =>
            (columns, rows, uniform.Type) switch
            {
                (2, 2, UniformType.FloatMat2) => true,
                (3, 3, UniformType.FloatMat3) => true,
                (4, 4, UniformType.FloatMat4) => true,
                (2, 3, UniformType.FloatMat2x3) => true,
                (2, 4, UniformType.FloatMat2x4) => true,
                (3, 2, UniformType.FloatMat3x2) => true,
                (3, 4, UniformType.FloatMat3x4) => true,
                (4, 2, UniformType.FloatMat4x2) => true,
                (4, 3, UniformType.FloatMat4x3) => true,
                (2, 2, UniformType.DoubleMat2) => true,
                (3, 3, UniformType.DoubleMat3) => true,
                (4, 4, UniformType.DoubleMat4) => true,
                (2, 3, UniformType.DoubleMat2x3) => true,
                (2, 4, UniformType.DoubleMat2x4) => true,
                (3, 2, UniformType.DoubleMat3x2) => true,
                (3, 4, UniformType.DoubleMat3x4) => true,
                (4, 2, UniformType.DoubleMat4x2) => true,
                (4, 3, UniformType.DoubleMat4x3) => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is an int.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if an int type.</returns>
        public static bool IsBool(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.Bool => true,
                UniformType.BoolVec2 => true,
                UniformType.BoolVec3 => true,
                UniformType.BoolVec4 => true,
                _ => true
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is an int.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if an int type.</returns>
        public static bool IsInt(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.Int => true,
                UniformType.IntVec2 => true,
                UniformType.IntVec3 => true,
                UniformType.IntVec4 => true,
                UniformType.IntSampler1d => true,
                UniformType.IntSampler2d => true,
                UniformType.IntSampler3d => true,
                UniformType.IntSamplerCube => true,
                UniformType.IntSampler1dArray => true,
                UniformType.IntSampler2dArray => true,
                UniformType.IntSampler2dRect => true,
                UniformType.IntSamplerBuffer => true,
                UniformType.IntSampler2dMultisample => true,
                UniformType.IntSampler2dMultisampleArray => true,
                UniformType.IntSamplerCubeMapArray => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is an unsigned int.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if an unsigned int type.</returns>
        public static bool IsUnsignedInt(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.UnsignedInt => true,
                UniformType.UnsignedIntVec2 => true,
                UniformType.UnsignedIntVec3 => true,
                UniformType.UnsignedIntVec4 => true,
                UniformType.UnsignedIntSampler1d => true,
                UniformType.UnsignedIntSampler2d => true,
                UniformType.UnsignedIntSampler3d => true,
                UniformType.UnsignedIntSamplerCube => true,
                UniformType.UnsignedIntSampler1dArray => true,
                UniformType.UnsignedIntSampler2dArray => true,
                UniformType.UnsignedIntSampler2dRect => true,
                UniformType.UnsignedIntSamplerBuffer => true,
                UniformType.UnsignedIntSampler2dMultisample => true,
                UniformType.UnsignedIntSampler2dMultisampleArray => true,
                UniformType.UnsignedIntSamplerCubeMapArray => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is a float.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if a float type.</returns>
        public static bool IsFloat(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.Float => true,
                UniformType.FloatVec2 => true,
                UniformType.FloatVec3 => true,
                UniformType.FloatVec4 => true,
                UniformType.FloatMat2 => true,
                UniformType.FloatMat3 => true,
                UniformType.FloatMat4 => true,
                UniformType.FloatMat2x3 => true,
                UniformType.FloatMat2x4 => true,
                UniformType.FloatMat3x2 => true,
                UniformType.FloatMat3x4 => true,
                UniformType.FloatMat4x2 => true,
                UniformType.FloatMat4x3 => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is a double.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if a double type.</returns>
        public static bool IsDouble(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.Double => true,
                UniformType.DoubleVec2 => true,
                UniformType.DoubleVec3 => true,
                UniformType.DoubleVec4 => true,
                UniformType.DoubleMat2 => true,
                UniformType.DoubleMat3 => true,
                UniformType.DoubleMat4 => true,
                UniformType.DoubleMat2x3 => true,
                UniformType.DoubleMat2x4 => true,
                UniformType.DoubleMat3x2 => true,
                UniformType.DoubleMat3x4 => true,
                UniformType.DoubleMat4x2 => true,
                UniformType.DoubleMat4x3 => true,
                _ => false
            };

        /// <summary>
        /// True if the <see cref="Uniform"/> is a vec.
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <returns>True if a vec type.</returns>
        public static bool IsVec(this Uniform uniform) =>
            uniform.Type switch
            {
                UniformType.FloatVec2 => true,
                UniformType.FloatVec3 => true,
                UniformType.FloatVec4 => true,
                UniformType.IntVec2 => true,
                UniformType.IntVec3 => true,
                UniformType.IntVec4 => true,
                UniformType.BoolVec2 => true,
                UniformType.BoolVec3 => true,
                UniformType.BoolVec4 => true,
                UniformType.UnsignedIntVec2 => true,
                UniformType.UnsignedIntVec3 => true,
                UniformType.UnsignedIntVec4 => true,
                UniformType.DoubleVec2 => true,
                UniformType.DoubleVec3 => true,
                UniformType.DoubleVec4 => true,
                _ => false
            };


        /// <summary>
        /// True if the <see cref="Uniform"/> is a vec of the given dimensions. 
        /// </summary>
        /// <param name="uniform">The uniform to check.</param>
        /// <param name="dimensions">The dimensions to check.</param>
        /// <returns>True if a vec type.</returns>
        public static bool IsVec(this Uniform uniform, int dimensions) =>
            (dimensions, uniform.Type) switch
            {
                (2, UniformType.FloatVec2) => true,
                (3, UniformType.FloatVec3) => true,
                (4, UniformType.FloatVec4) => true,
                (2, UniformType.IntVec2) => true,
                (3, UniformType.IntVec3) => true,
                (4, UniformType.IntVec4) => true,
                (2, UniformType.BoolVec2) => true,
                (3, UniformType.BoolVec3) => true,
                (4, UniformType.BoolVec4) => true,
                (2, UniformType.UnsignedIntVec2) => true,
                (3, UniformType.UnsignedIntVec3) => true,
                (4, UniformType.UnsignedIntVec4) => true,
                (2, UniformType.DoubleVec2) => true,
                (3, UniformType.DoubleVec3) => true,
                (4, UniformType.DoubleVec4) => true,
                _ => false
            };

        /// <summary>
        /// Sets teh uniform to the value
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The value.</param>
        public static void Set(this Uniform uniform, in Matrix4 a0)
        {
            GLException.ThrowPreceding();
            GLX.UniformMatrix4f(uniform.Location, a0);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to matrix");
        }

        /// <summary>
        /// Sets teh uniform to the value
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The value.</param>
        public static void Set(this Uniform uniform, in TextureUnit a0)
        {
            GL.Uniform1i(uniform.Location, (int) a0 - (int) TextureUnit.Texture0);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to texture unit");
        }


        /// <summary>
        /// Sets teh uniform to the value
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The value.</param>
        public static void Set(this Uniform uniform, in int a0)
        {
            GLException.ThrowPreceding();
            GL.Uniform1i(uniform.Location, a0);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to integer");
        }

        /// <summary>
        /// Sets teh uniform to the values
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The first value.</param>
        /// <param name="a1">The second value.</param>
        public static void Set(this Uniform uniform, in int a0, in int a1)
        {
            GLException.ThrowPreceding();
            GL.Uniform2i(uniform.Location, a0, a1);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to integers");
        }

        /// <summary>
        /// Sets teh uniform to the values
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The first value.</param>
        /// <param name="a1">The second value.</param>
        /// <param name="a2">The third value.</param>
        public static void Set(this Uniform uniform, in int a0, in int a1, in int a2)
        {
            GLException.ThrowPreceding();
            GL.Uniform3i(uniform.Location, a0, a1, a2);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to integers");
        }

        /// <summary>
        /// Sets teh uniform to the values
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The first value.</param>
        /// <param name="a1">The second value.</param>
        /// <param name="a2">The third value.</param>
        /// <param name="a3">The fourth value.</param>
        public static void Set(this Uniform uniform, in int a0, in int a1, in int a2, in int a3)
        {
            GLException.ThrowPreceding();
            GL.Uniform4i(uniform.Location, a0, a1, a2, a3);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to integers");
        }


        /// <summary>
        /// Sets teh uniform to the value
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The value.</param>
        public static void Set(this Uniform uniform, in float a0)
        {
            GLException.ThrowPreceding();
            GL.Uniform1f(uniform.Location, a0);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to float");
        }

        /// <summary>
        /// Sets teh uniform to the values
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The first value.</param>
        /// <param name="a1">The second value.</param>
        public static void Set(this Uniform uniform, in float a0, in float a1)
        {
            GLException.ThrowPreceding();
            GL.Uniform2f(uniform.Location, a0, a1);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to floats");
        }

        /// <summary>
        /// Sets teh uniform to the values
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The first value.</param>
        /// <param name="a1">The second value.</param>
        /// <param name="a2">The third value.</param>
        public static void Set(this Uniform uniform, in float a0, in float a1, in float a2)
        {
            GLException.ThrowPreceding();
            GL.Uniform3f(uniform.Location, a0, a1, a2);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to floats");
        }

        /// <summary>
        /// Sets teh uniform to the values
        /// </summary>
        /// <param name="uniform">The uniform to assign.</param>
        /// <param name="a0">The first value.</param>
        /// <param name="a1">The second value.</param>
        /// <param name="a2">The third value.</param>
        /// <param name="a3">The fourth value.</param>
        public static void Set(this Uniform uniform, in float a0, in float a1, in float a2, in float a3)
        {
            GLException.ThrowPreceding();
            GL.Uniform4f(uniform.Location, a0, a1, a2, a3);
            GLException.ThrowGlErrors($"Error setting {uniform.Name} to floats");
        }
    }
}