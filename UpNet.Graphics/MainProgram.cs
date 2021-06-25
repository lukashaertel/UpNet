using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using UpNet.Graphics.Graphics;
using UpNet.Graphics.Graphics.Attributes;

namespace UpNet.Graphics
{
    public class MainProgram : LegacyProgram
    {
        [Uniform("uModel")] public int Model { get; }
        [Uniform("uPV")] public int ProjectionView { get; }

        [Uniform("uTint")] public int Tint { get; }

        public void LoadModel(Matrix4 matrix) =>
            GLX.UniformMatrix4f(Model, matrix);

        public void LoadProjectionView(Matrix4 matrix) =>
            GLX.UniformMatrix4f(ProjectionView, matrix);

        public void LoadTint(float r, float g, float b, float a) =>
            GL.Uniform4f(Tint, r, g, b, a);

        [Uniform("uTexture")] public int Texture { get; }


        public void LoadTexture(TextureUnit unit)
        {
            var index = (int) unit - (int) TextureUnit.Texture0;
            GL.Uniform1i(Texture, index);
        }

        [Uniform("uHeight")] public int Heightmap { get; }

        public void LoadHeightmap(TextureUnit unit)
        {
            var index = (int) unit - (int) TextureUnit.Texture0;
            GL.Uniform1i(Heightmap, index);
        }

        [Uniform("uHeightScale")] public int HeightmapScale { get; }

        public void LoadHeightmapScale(float scale) =>
            GL.Uniform1f(HeightmapScale, scale);

        [Attrib("aPosition")] public int AttPosition { get; }
        [Attrib("aBitangent")] public int AttBitangent { get; }
        [Attrib("aTangent")] public int AttTangent { get; }

        [Attrib("aNormal")] public int AttNormal { get; }

        [Attrib("aTexCoord")] public int AttTexCoord { get; }

        [Attrib("aColor")] public int AttColor { get; }

        public MainProgram() : base()
        {
        }
    }
}