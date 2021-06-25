using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace UpNet.Graphics
{
    public static class GLX
    {
        public static void UniformMatrix4f(int location, in Matrix4 matrix) =>
            UniformMatrix4f(location, false, matrix);

        public static void UniformMatrix4f(int location, bool transpose, in Matrix4 matrix)
        {
            GL.UniformMatrix4f(location, 1, transpose, stackalloc[]
            {
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            });
        }
    }
}