using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using UpNet.Graphics.Graphics;
using UpNet.Graphics.Graphics.Attributes;

namespace UpNet.Graphics
{
    public class FontProgram : LegacyProgram
    {
        [Uniform("uModel")] public int Model { get; }
        [Uniform("uPV")] public int ProjectionView { get; }

        public void LoadModel(Matrix4 matrix) =>
            GLX.UniformMatrix4f(Model, matrix);

        public void LoadProjectionView(Matrix4 matrix) =>
            GLX.UniformMatrix4f(ProjectionView, matrix);

        [Uniform("uTexture")] public int Texture { get; }

        public void LoadTexture(TextureUnit unit)
        {
            var index = (int) unit - (int) TextureUnit.Texture0;
            GL.Uniform1i(Texture, index);
        }

        [Uniform("uDistanceFactor")] public int DistanceFactor { get; }

        public void LoadGlyphSettings(float glyphSize, float distanceRange, float size)
        {
            GL.Uniform1f(DistanceFactor, (float) ((double) distanceRange * size / glyphSize));
        }

        [Uniform("uColor")] public int Color { get; }

        public void LoadColor(float r, float g, float b, float a = 1f)
        {
            GL.Uniform4f(Color, r, g, b, a);
        }

        [Uniform("uFontWeight")] public int FontWeight { get; }

        public void LoadWeight(float weight = 0.0f)
        {
            GL.Uniform1f(FontWeight, weight);
        }

        [Uniform("uShadowClipped")] public int ShadowClipped { get; }
        [Uniform("uShadowColor")] public int ShadowColor { get; }
        [Uniform("uShadowOffset")] public int ShadowOffset { get; }
        [Uniform("uShadowSmoothing")] public int ShadowSmoothing { get; }

        public void LoadShadow(float r, float g, float b, float a, Vector2 shadowOffset,
            float smoothing = 0.1f, bool clipped = false)
        {
            GL.Uniform4f(ShadowColor, r, g, b, a);
            GL.Uniform2f(ShadowOffset, shadowOffset);
            GL.Uniform1f(ShadowSmoothing, smoothing);
            GL.Uniform1f(ShadowClipped, clipped ? 1f : 0f);
        }

        [Uniform("uInnerShadowColor")] public int InnerShadowColor { get; }
        [Uniform("uInnerShadowRange")] public int InnerShadowRange { get; }


        public void LoadInnerShadow(float r, float g, float b, float a, float range)
        {
            GL.Uniform4f(InnerShadowColor, r, g, b, a);
            GL.Uniform1f(InnerShadowRange, range);
        }

        [Attrib("aPosition")] public int AttPosition { get; }

        [Attrib("aTexCoord")] public int AttTexCoord { get; }
        [Attrib("aColor")] public int AttColor { get; }
    }
}