using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Cyotek.Drawing.BitmapFont;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
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

        // private MainProgram Program { get; set; }

        private ManagedProgram Program { get; set; }

        private FontProgram FontProgram { get; set; }
        private MainVertexArray Planet { get; set; }
        private FontVertexArray Text { get; set; }

        private ManagedTexture2D Ground { get; set; }
        private ManagedTexture2D Clouds { get; set; }

        private ManagedTexture2D Heightmap { get; set; }
        private ManagedTexture2D Cantarell { get; set; }

        private Matrix4 Project { get; set; }
        private Matrix4 View { get; set; }

        private Matrix4 ProjectionView { get; set; }
        private Vector3 Eye { get; set; } = Vector3.UnitZ * -4f;

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

            FontProgram = new FontProgram();
            FontProgram.Attach(await LegacyProgramArg.FromPathsAsync("Resources/Font.vert", "Resources/Font.frag"));
            using (FontProgram.Bind())
            {
                FontProgram.LoadTexture(TextureUnit.Texture0);
                FontProgram.LoadGlyphSettings(5f, 32f, 32f);
            }

            // Load font.
            var font = BitmapFontLoader.LoadFontFromTextFile("Resources/Cantarell-Regular.fnt");
            var fontImagePath = font.Pages.Single().FileName;

            // Load all images.
            var (worldImage, cloudsImage, heightmapImage, cantarellImage) = await Task.WhenAll(
                Image.LoadAsync<Rgba32>("Resources/world.png"),
                Image.LoadAsync<Rgba32>("Resources/clouds.png"),
                Image.LoadAsync<Rgba32>("Resources/worldheight.png"),
                Image.LoadAsync<Rgba32>(fontImagePath));

            // Create textures.
            Ground = Images.Load2DFrom(worldImage);
            Clouds = Images.Load2DFrom(cloudsImage);
            Heightmap = Images.Load2DFrom(heightmapImage);
            Cantarell = Images.Load2DFrom(cantarellImage);

            var pos = Icosphere.DataPos;
            var ind = Icosphere.DataIndices;

            Planet = new MainVertexArray();
            using (Planet.Bind())
            {
                Planet.Attach(Program);
                Planet.LoadPositionData(pos);
                Planet.LoadTexCoordData(Icosphere.ComputeTexCoords(pos));
                Planet.LoadNormalData(Icosphere.ComputeNormals(pos));
                Planet.LoadBitangentData(Icosphere.ComputeBitangents(pos));
                Planet.LoadTangentData(Icosphere.ComputeTangents(pos));
                Planet.LoadColorData(Icosphere.ComputeColors(pos));
                Planet.LoadElementsData(ind);
            }

            Text = new FontVertexArray();
            using (Text.Bind())
            {
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

                Text.Attach(FontProgram);
                Text.LoadPositionData(positions.ToArray());
                Text.LoadColorData(colors.ToArray());
                Text.LoadTexCoordData(texCoords.ToArray());
                Text.LoadElementsData(indices.ToArray());
            }

            Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < 6; i++)
                {
                    await Task.Delay(1000);
                    (pos, ind) = Icosphere.SubdivideNoReuse(pos, ind);
                    await Dispatcher.EnqueueAsync(delegate
                    {
                        using (Planet.Bind())
                        {
                            Planet.LoadPositionData(pos);
                            Planet.LoadTexCoordData(Icosphere.ComputeTexCoords(pos));
                            Planet.LoadNormalData(Icosphere.ComputeNormals(pos));
                            Planet.LoadBitangentData(Icosphere.ComputeBitangents(pos));
                            Planet.LoadTangentData(Icosphere.ComputeTangents(pos));
                            Planet.LoadColorData(Icosphere.ComputeColors(pos));
                            Planet.LoadElementsData(ind);
                        }
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

            GL.ActiveTexture(TextureUnit.Texture1);
            Heightmap.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);

            // Use shader. Set matrix to combined view and draw.
            Program.Use();
            using (Planet.Bind())
            {
                Program["uModel"].Set(Matrix4.CreateRotationX(0.3f) *
                                      Matrix4.CreateRotationY(totalSeconds / 60f));
                Program["uPV"].Set(ProjectionView);
                Program["uTint"].Set(1f, 1f, 1f, 1f);
                Program["uHeightScale"].Set(0.02f);

                GL.Enable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);

                Ground.Bind();
                Planet.Draw();
                Ground.Unbind();

                GL.Disable(EnableCap.CullFace);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                Program["uHeightScale"].Set(0.0f);

                Clouds.Bind();
                const float min = 0.005f;
                const float max = 0.015f;
                const float step = 0.0005f;

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
                    Planet.Draw();
                }
            }

            ManagedProgram.UseNone();

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);
            Cantarell.Bind();

            using (FontProgram.Bind())
            using (Text.Bind())
            using (LegacyTexture.Activate(TextureUnit.Texture0))
            {
                FontProgram.LoadModel(Matrix4.CreateRotationY(MathHelper.PiOver2) * Matrix4.CreateScale(1 / 200f) *
                                      Matrix4.CreateTranslation(1.2f, 0f, 0f));
                FontProgram.LoadProjectionView(ProjectionView);
                Text.Draw();
            }

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

    public struct StackData
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
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