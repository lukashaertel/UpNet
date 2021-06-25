using System;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Graphics;
using UpNet.Graphics.Graphics.Attributes;

namespace UpNet.Graphics
{
    public static class VertexArrays
    {
        public static int LoadBuffer<T>(BufferTargetARB target, uint buffer, int length, Span<T> data)
            where T : unmanaged
        {
            GL.BindBuffer(target, buffer);
            if (length == data.Length)
                GL.BufferSubData(target, IntPtr.Zero, data);
            else
                GL.BufferData(target, data, BufferUsageARB.StaticDraw);

            return data.Length;
        }
    }

    class MainVertexArray : LegacyVertexArray
    {
        [Buffer]
        [Layout("aPosition", 3, VertexAttribPointerType.Float)]
        public uint Position { get; }

        public int PositionLength { get; private set; }

        public void LoadPositionData(float[] data) =>
            PositionLength =
                VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Position, PositionLength, data.AsSpan());


        [Buffer]
        [Layout("aTexCoord", 2, VertexAttribPointerType.Float)]
        public uint TexCoord { get; }

        public int TexCoordLength { get; private set; }

        public void LoadTexCoordData(float[] data) =>
            TexCoordLength =
                VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, TexCoord, TexCoordLength, data.AsSpan());

        [Buffer]
        [Layout("aNormal", 3, VertexAttribPointerType.Float)]
        public uint Normal { get; }

        public int NormalLength { get; private set; }

        public void LoadNormalData(float[] data) =>
            NormalLength = VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Normal, NormalLength, data.AsSpan());


        [Buffer]
        [Layout("aBitangent", 3, VertexAttribPointerType.Float)]
        public uint Bitangent { get; }

        public int BitangentLength { get; private set; }

        public void LoadBitangentData(float[] data) =>
            BitangentLength =
                VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Bitangent, BitangentLength, data.AsSpan());


        [Buffer]
        [Layout("aTangent", 3, VertexAttribPointerType.Float)]
        public uint Tangent { get; }

        public int TangentLength { get; private set; }

        public void LoadTangentData(float[] data) =>
            TangentLength = VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Tangent, TangentLength, data.AsSpan());


        [Buffer]
        [Layout("aColor", 3, VertexAttribPointerType.UnsignedByte, true)]
        public uint Color { get; }

        public int ColorLength { get; private set; }

        public void LoadColorData(byte[] data) =>
            ColorLength = VertexArrays.LoadBuffer(BufferTargetARB.ArrayBuffer, Color, ColorLength, data.AsSpan());

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