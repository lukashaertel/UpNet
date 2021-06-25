using System;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Graphics;
using UpNet.Graphics.Graphics.Attributes;

namespace UpNet.Graphics
{
    public class FontVertexArray : LegacyVertexArray
    {
        [Buffer]
        [Layout(typeof(FontProgram), nameof(FontProgram.AttPosition), 3, VertexAttribPointerType.Float)]
        public uint Position { get; }

        public int PositionLength { get; private set; }

        public void LoadPositionData(float[] data) =>
            PositionLength =
                VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Position, PositionLength, data.AsSpan());

        [Buffer]
        [Layout(typeof(FontProgram), nameof(FontProgram.AttTexCoord), 2, VertexAttribPointerType.Float)]
        public uint TexCoord { get; }

        public int TexCoordLength { get; private set; }

        public void LoadTexCoordData(float[] data) =>
            TexCoordLength =
                VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, TexCoord, TexCoordLength, data.AsSpan());


        [Buffer]
        [Layout(typeof(FontProgram), nameof(FontProgram.AttColor), 4, VertexAttribPointerType.UnsignedByte, true)]
        public uint Color { get; }

        public int ColorLength { get; private set; }

        public void LoadColorData(byte[] data) =>
            ColorLength = VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Color, ColorLength, data.AsSpan());

        public void LoadColorData(uint[] data) =>
            ColorLength = VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Color, ColorLength / 4, data.AsSpan());

        [Buffer] public uint Elements { get; }
        public int ElementsLength { get; private set; }

        public void LoadElementsData(uint[] data) =>
            ElementsLength = VertexArrays.LoadBuffer(BufferTargetARB.ElementArrayBuffer, Elements, ElementsLength,
                data.AsSpan());

        public void Draw()
        {
            GL.DrawElements(PrimitiveType.Triangles, ElementsLength, DrawElementsType.UnsignedInt, 0);
        }
    }
}