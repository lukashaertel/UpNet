using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Cyotek.Drawing.BitmapFont;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using UpNet.Graphics.Graphics;
using UpNet.Graphics.Graphics.Buffers;
using UpNet.Graphics.Graphics.Loading;
using UpNet.Graphics.Graphics.Shading;
using UpNet.Graphics.Graphics.Textures;
using UpNet.Graphics.Graphics.VertexArray;
using UpNet.Graphics.Lang;
using UpNet.Graphics.Threading;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using Image = SixLabors.ImageSharp.Image;

namespace UpNet.Graphics
{
    class Game : GameWindow
    {
        private static readonly GameWindowSettings GameWindowSettings = new()
        {
            RenderFrequency = 60.0,
            UpdateFrequency = 60.0
        };

        private static readonly NativeWindowSettings NativeWindowSettings = new()
        {
            Size = (1920, 1080)
        };

        private DateTime Started { get; }

        private GameWindowDispatcher Dispatcher { get; }

        public Game() : base(GameWindowSettings, NativeWindowSettings)
        {
            Dispatcher = new GameWindowDispatcher(this);
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher));
            Started = DateTime.Now;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
                Close();

            base.OnKeyDown(e);
        }

        private ManagedProgram Program { get; set; }
        private ManagedProgram SplineProgram { get; set; }
        private ManagedVertexArray Planet { get; set; }

        private ManagedBuffer PlanetPosition { get; set; }
        private ManagedBuffer PlanetTexCoord { get; set; }
        private ManagedBuffer PlanetNormal { get; set; }
        private ManagedBuffer PlanetBitangent { get; set; }
        private ManagedBuffer PlanetTangent { get; set; }
        private ManagedBuffer PlanetColor { get; set; }
        private ManagedBuffer PlanetElements { get; set; }
        private ManagedProgram FontProgram { get; set; }
        private ManagedVertexArray Text { get; set; }
        private ManagedBuffer TextPosition { get; set; }
        private ManagedBuffer TextTexCoord { get; set; }
        private ManagedBuffer TextColor { get; set; }
        private ManagedBuffer TextElements { get; set; }


        private ManagedTexture2D Noise { get; set; }
        private ManagedTexture2D Ground { get; set; }
        private ManagedTexture2D Clouds { get; set; }

        private ManagedTexture2D Heightmap { get; set; }
        private ManagedTexture2D Cantarell { get; set; }

        private Matrix4 Project { get; set; }
        private Matrix4 View { get; set; }

        private Matrix4 ProjectionView { get; set; }
        private Vector3 Eye { get; set; } = Vector3.UnitZ * -4f;

        private byte[] MakeNoise(int w, int h)
        {
            var perms1 = new byte[256];
            new Random(0).NextBytes(perms1);
            var perms2 = new byte[256];
            new Random(1).NextBytes(perms2);
            var perms3 = new byte[256];
            new Random(2).NextBytes(perms3);
            var sn1 = new SimplexNoise(perms1);
            var sn2 = new SimplexNoise(perms2);
            var sn3 = new SimplexNoise(perms3);
            var noise = new byte[w * h * 3];

            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                noise[(y * w + x) * 3 + 0] = (byte) (sn1.Eval(x, y) * 255f);
                noise[(y * w + x) * 3 + 1] = (byte) (sn2.Eval(x, y) * 255f);
                noise[(y * w + x) * 3 + 2] = (byte) (sn3.Eval(x, y) * 255f);
            }

            return noise;
        }

        private async Task LoadAsync()
        {
            using (var buf = new ManagedBuffer(BufferTargetARB.ArrayBuffer))
            {
                const int ints = 4;

                buf.SizeInBytes = 4 * ints;
                buf.WriteDataSlice(new[] {1, 2}, 0 * ints);
                buf.WriteDataSlice(new[] {3, 4}, 2 * ints);
                var data = buf.ReadAllData<int>();

                Console.WriteLine(data);
            }

            // Load shaders.
            var (shaderVert, shaderFrag) = await Task.WhenAll(
                File.ReadAllTextAsync("Resources/Shader.vert"),
                File.ReadAllTextAsync("Resources/Shader.frag"));

            Program = new ManagedProgram();
            Program.AttachShader(ShaderType.VertexShader, shaderVert);
            Program.AttachShader(ShaderType.FragmentShader, shaderFrag);
            Program.Link();
            Program.Use();
            Program["uTexture"].Set(TextureUnit.Texture0);
            Program["uHeight"].Set(TextureUnit.Texture1);
            Program.Disuse();

            // Load shaders.
            (shaderVert, shaderFrag) = await Task.WhenAll(
                File.ReadAllTextAsync("Resources/Spline.vert"),
                File.ReadAllTextAsync("Resources/Spline.frag"));

            SplineProgram = new ManagedProgram();
            SplineProgram.AttachShader(ShaderType.VertexShader, shaderVert);
            SplineProgram.AttachShader(ShaderType.FragmentShader, shaderFrag);
            SplineProgram.Link();
            SplineProgram.Use();
            SplineProgram["uTint"].Set(1f, 1f, 1f, 1f);
            SplineProgram["uHeight"].Set(TextureUnit.Texture1);
            SplineProgram["uNoise"].Set(TextureUnit.Texture2);
            SplineProgram["uHeightOffset"].Set(0.1f);
            SplineProgram["uSpline"].Set(Matrix4.CreateScale(10.0f, 1.0f, 10.0f));
            SplineProgram.Disuse();

            // Load shaders.
            (shaderVert, shaderFrag) = await Task.WhenAll(
                File.ReadAllTextAsync("Resources/Font.vert"),
                File.ReadAllTextAsync("Resources/Font.frag"));

            FontProgram = new ManagedProgram();
            FontProgram.AttachShader(ShaderType.VertexShader, shaderVert);
            FontProgram.AttachShader(ShaderType.FragmentShader, shaderFrag);
            FontProgram.Link();
            FontProgram.Use();
            FontProgram["uTexture"].Set(TextureUnit.Texture0);
            FontProgram["uDistanceFactor"].Set(32f * 32f / 5f);
            FontProgram.Disuse();

            // Load font.
            var font = BitmapFontLoader.LoadFontFromTextFile("Resources/Cantarell-Regular.fnt");
            var fontImagePath = font.Pages.Single().FileName;

            // Load all images.
            var (worldImage, cloudsImage, heightmapImage, cantarellImage) = await Task.WhenAll(
                Image.LoadAsync<Rgba32>("Resources/world.png"),
                Image.LoadAsync<Rgba32>("Resources/clouds.png"),
                Image.LoadAsync<Rgba32>("Resources/worldheight.png"),
                Image.LoadAsync<Rgba32>(fontImagePath));


            Noise = new ManagedTexture2D(SizedInternalFormat.Rgb8, 512, 512);
            Noise.WriteData(MakeNoise(Noise.Width, Noise.Height), PixelFormat.Rgb, PixelType.UnsignedByte);
            Noise.WrapS = TextureWrapMode.MirroredRepeat;
            Noise.WrapT = TextureWrapMode.MirroredRepeat;
            Noise.GenerateMipMaps();

            // Create textures.
            Ground = Images.Load2DFrom(worldImage);
            Clouds = Images.Load2DFrom(cloudsImage);
            Heightmap = Images.Load2DFrom(heightmapImage);
            Cantarell = Images.Load2DFrom(cantarellImage);

            var pos = Icosphere.DataPos;
            var ind = Icosphere.DataIndices;

            PlanetPosition = new ManagedBuffer();
            PlanetPosition.WriteData(pos);
            PlanetTexCoord = new ManagedBuffer();
            PlanetTexCoord.WriteData(Icosphere.ComputeTexCoords(pos));
            PlanetNormal = new ManagedBuffer();
            PlanetNormal.WriteData(Icosphere.ComputeNormals(pos));
            PlanetBitangent = new ManagedBuffer();
            PlanetBitangent.WriteData(Icosphere.ComputeBitangents(pos));
            PlanetTangent = new ManagedBuffer();
            PlanetTangent.WriteData(Icosphere.ComputeTangents(pos));
            PlanetColor = new ManagedBuffer();
            PlanetColor.WriteData(Icosphere.ComputeColors(pos));
            PlanetElements = new ManagedBuffer(BufferTargetARB.ElementArrayBuffer);
            PlanetElements.WriteData(ind);


            var attribPosition = Program.FindRequiredAttrib("aPosition");
            var attribTexCoord = Program.FindRequiredAttrib("aTexCoord");
            var attribNormal = Program.FindRequiredAttrib("aNormal");
            var attribBitangent = Program.FindRequiredAttrib("aBitangent");
            var attribTangent = Program.FindRequiredAttrib("aTangent");
            var attribColor = Program.FindRequiredAttrib("aColor");

            Planet = new ManagedVertexArray();
            Planet.Bind();
            Planet.Enable(attribPosition);
            Planet.Enable(attribTexCoord);
            Planet.Enable(attribNormal);
            Planet.Enable(attribBitangent);
            Planet.Enable(attribTangent);
            Planet.Enable(attribColor);

            PlanetPosition.Bind();
            Planet.Apply(attribPosition, new PointerLayout(3, VertexAttribPointerType.Float));
            PlanetTexCoord.Bind();
            Planet.Apply(attribTexCoord, new PointerLayout(2, VertexAttribPointerType.Float));
            PlanetNormal.Bind();
            Planet.Apply(attribNormal, new PointerLayout(3, VertexAttribPointerType.Float));
            PlanetBitangent.Bind();
            Planet.Apply(attribBitangent, new PointerLayout(3, VertexAttribPointerType.Float));
            PlanetTangent.Bind();
            Planet.Apply(attribTangent, new PointerLayout(3, VertexAttribPointerType.Float));
            PlanetColor.Bind();
            Planet.Apply(attribColor, new PointerLayout(3, VertexAttribPointerType.UnsignedByte, true));
            PlanetElements.Bind();
            Planet.Unbind();

            Text = new ManagedVertexArray();
            Text.Bind();
            var text = "Hell world";
            var positions = new List<float>();
            var colors = new List<uint>();
            var texCoords = new List<float>();
            var indices = new List<uint>();

            var x = 0;
            var y = 0;
            var previousCharacter = ' ';
            var space = font['i'].Width;

            foreach (var character in text)
            {
                switch (character)
                {
                    case '\n':
                        x = 0;
                        y -= font.LineHeight;
                        break;

                    case '\r':
                        break;

                    case ' ':
                        x += space;
                        break;

                    case '\t':
                        x += 4 * space;
                        break;

                    default:
                        var data = font[character];
                        var kerning = font.GetKerning(previousCharacter, character);

                        if (!data.IsEmpty)
                        {
                            indices.Add((uint) (positions.Count / 3 + 0));
                            indices.Add((uint) (positions.Count / 3 + 1));
                            indices.Add((uint) (positions.Count / 3 + 2));
                            indices.Add((uint) (positions.Count / 3 + 2));
                            indices.Add((uint) (positions.Count / 3 + 3));
                            indices.Add((uint) (positions.Count / 3 + 0));

                            var cx = x + data.XOffset + kerning;
                            var cy = y + data.YOffset;
                            positions.Add(cx);
                            positions.Add(-cy - data.Height);
                            positions.Add(0f);

                            positions.Add(cx);
                            positions.Add(-cy);
                            positions.Add(0f);

                            positions.Add(cx + data.Width);
                            positions.Add(-cy);
                            positions.Add(0f);

                            positions.Add(cx + data.Width);
                            positions.Add(-cy - data.Height);
                            positions.Add(0f);

                            texCoords.Add(data.X / (float) cantarellImage.Width);
                            texCoords.Add(1f - (data.Y + data.Height) / (float) cantarellImage.Height);
                            texCoords.Add(data.X / (float) cantarellImage.Width);
                            texCoords.Add(1f - data.Y / (float) cantarellImage.Height);
                            texCoords.Add((data.X + data.Width) / (float) cantarellImage.Width);
                            texCoords.Add(1f - data.Y / (float) cantarellImage.Height);
                            texCoords.Add((data.X + data.Width) / (float) cantarellImage.Width);
                            texCoords.Add(1f - (data.Y + data.Height) / (float) cantarellImage.Height);

                            colors.Add(0xffffffff);
                            colors.Add(0xffffffff);
                            colors.Add(0xffffffff);
                            colors.Add(0xffffffff);

                            x += data.XAdvance + kerning;
                        }

                        break;
                }

                previousCharacter = character;
            }

            var attribFontPosition = FontProgram.FindRequiredAttrib("aPosition");
            var attribFontTexCoord = FontProgram.FindRequiredAttrib("aTexCoord");
            var attribFontColor = FontProgram.FindRequiredAttrib("aColor");

            Text.Enable(attribFontPosition);
            TextPosition = new ManagedBuffer();
            TextPosition.Bind();
            TextPosition.WriteData(positions.ToArray());
            Text.Apply(attribFontPosition, new PointerLayout(3, VertexAttribPointerType.Float));

            Text.Enable(attribFontTexCoord);
            TextTexCoord = new ManagedBuffer();
            TextTexCoord.Bind();
            TextTexCoord.WriteData(texCoords.ToArray());
            Text.Apply(attribFontTexCoord, new PointerLayout(2, VertexAttribPointerType.Float));

            Text.Enable(attribFontColor);
            TextColor = new ManagedBuffer();
            TextColor.Bind();
            TextColor.WriteData(colors.ToArray());
            Text.Apply(attribFontColor, new PointerLayout(4, VertexAttribPointerType.UnsignedByte, normalized: true));

            TextElements = new ManagedBuffer(BufferTargetARB.ElementArrayBuffer);
            TextElements.Bind();
            TextElements.WriteData(indices.ToArray());
            Text.Unbind();

            Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < 4; i++)
                {
                    await Task.Delay(1000);
                    (pos, ind) = Icosphere.SubdivideNoReuse(pos, ind);
                    await Dispatcher.EnqueueAsync(delegate
                    {
                        PlanetPosition.WriteData(pos);
                        PlanetTexCoord.WriteData(Icosphere.ComputeTexCoords(pos));
                        PlanetNormal.WriteData(Icosphere.ComputeNormals(pos));
                        PlanetBitangent.WriteData(Icosphere.ComputeBitangents(pos));
                        PlanetTangent.WriteData(Icosphere.ComputeTangents(pos));
                        PlanetColor.WriteData(Icosphere.ComputeColors(pos));
                        PlanetElements.WriteData(ind);
                    });
                }
            }, default, default, TaskScheduler.Default);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1.0);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            View = Matrix4.LookAt(Eye, Vector3.Zero, Vector3.UnitY);
            ProjectionView = View * Project;

            Dispatcher.ActiveGetResult(Task.Factory.StartNew(
                LoadAsync, default, default,
                TaskScheduler.FromCurrentSynchronizationContext()).Unwrap());
        }

        private bool Wireframe { get; set; }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Tab)
                if (!Wireframe)
                {
                    Wireframe = true;
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                else
                {
                    Wireframe = false;
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }

            base.OnKeyUp(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var updated = false;
            if (KeyboardState[Keys.E] && (updated = true))
                Eye *= 0.99f;
            if (KeyboardState[Keys.Q] && (updated = true))
                Eye *= 1.01f;
            if (KeyboardState[Keys.A] && (updated = true))
                Eye = Vector3.Transform(Eye, Quaternion.FromAxisAngle(Vector3.UnitY, -0.01f));
            if (KeyboardState[Keys.D] && (updated = true))
                Eye = Vector3.Transform(Eye, Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f));

            if (KeyboardState[Keys.W] && (updated = true))
                Eye = Vector3.Transform(Eye, Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitY, Eye), -0.01f));
            if (KeyboardState[Keys.S] && (updated = true))
                Eye = Vector3.Transform(Eye, Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitY, Eye), 0.01f));

            if (updated)
            {
                View = Matrix4.LookAt(Eye, Vector3.Zero, Vector3.UnitY);
                ProjectionView = View * Project;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            var totalSeconds = (float) (DateTime.Now - Started).TotalSeconds;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture2);
            Noise.Bind();
            GL.ActiveTexture(TextureUnit.Texture1);
            Heightmap.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            Ground.Bind();

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            SplineProgram.Use();
            SplineProgram["uModel"].Set(Matrix4.CreateRotationX(0.3f) *
                                        Matrix4.CreateRotationY(totalSeconds / 60f));
            SplineProgram["uPV"].Set(ProjectionView);
            SplineProgram["uHeightScale"].Set(0.02f);

            Planet.Bind();
            GL.DrawElements(
                PrimitiveType.Triangles,
                PlanetElements.SizeInBytes / 4,
                DrawElementsType.UnsignedInt, 0);
            Planet.Unbind();
            SplineProgram.Disuse();

            // Use shader. Set matrix to combined view and draw.
            Program.Use();
            Program["uModel"].Set(Matrix4.CreateRotationX(0.3f) *
                                  Matrix4.CreateRotationY(totalSeconds / 60f));
            Program["uPV"].Set(ProjectionView);
            Program["uTint"].Set(1f, 1f, 1f, 1f);
            Program["uHeightScale"].Set(0.02f);

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            Planet.Bind();
            GL.DrawElements(
                PrimitiveType.Triangles,
                PlanetElements.SizeInBytes / 4,
                DrawElementsType.UnsignedInt, 0);

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            Program["uHeightScale"].Set(0.0f);

            Clouds.Bind();
            const float min = 0.005f;
            const float max = 0.015f;
            const float step = 0.001f;

            const float m = (min + max) / 2f;
            const float d = max - m;
            const float a = (max - min) / step;
            for (var o = min; o <= max; o += step)
            {
                var x = (o - m) / d;
                var factor = MathHelper.Min(1f, 1.4f - x * x * x * x);

                Program["uTint"].Set(1f, 1f, 1f, factor / a);
                Program["uModel"].Set(Matrix4.CreateRotationX(0.3f) *
                                      Matrix4.CreateScale(1.00f + o) *
                                      Matrix4.CreateRotationY(totalSeconds * 1.4f / 60f));

                GL.DrawElements(
                    PrimitiveType.Triangles,
                    PlanetElements.SizeInBytes / 4,
                    DrawElementsType.UnsignedInt, 0);
            }

            Ground.Unbind();
            Planet.Unbind();

            Program.Disuse();

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);
            Cantarell.Bind();

            FontProgram.Use();
            Text.Bind();
            FontProgram["uModel"].Set(Matrix4.CreateRotationY(MathHelper.PiOver2) * Matrix4.CreateScale(1 / 200f) *
                                      Matrix4.CreateTranslation(1.2f, 0f, 0f));
            FontProgram["uPV"].Set(ProjectionView);

            GL.DrawElements(
                PrimitiveType.Triangles,
                TextElements.SizeInBytes / 4,
                DrawElementsType.UnsignedInt, 0);

            Text.Unbind();
            FontProgram.Disuse();

            // Swap and delegate back to base.
            Context.SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            // Dispose of resources.
            Program.Dispose();
            Planet.Dispose();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // Update viewport.
            GL.Viewport(0, 0, e.Width, e.Height);

            // Update projection.
            Project = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                e.Width / (float) e.Height, 0.01f, 100.0f);
            ProjectionView = View * Project;

            base.OnResize(e);
        }
    }

    class GameProgram
    {
        static void Main(string[] args)
        {
            using var game = new Game();
            game.Run();
        }
    }
}