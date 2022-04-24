using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTKTesting.Rendering;
using OpenTKTesting.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace OpenTKTesting.Game;

public class SimpleSpriteGame : GameWindow
{
    private ShaderProgram _program;
    private VertexArrayObject _vao;
    private TextureHandle _textureId;
    private TextureHandle _textureAlphaChannelId;

    public SimpleSpriteGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "../../../Shaders/sprite_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/sprite_frag.glsl"),
        });

        _vao = new VertexArrayObject();

        _ = new VertexBufferObject(new float[]
        {
            -1, -1, 0, 1,
            1, -1, 1, 1,
            1, 1, 1, 0,
            -1, 1, 0, 0
        });
        _ = new ElementBufferObject(new uint[]
        {
            0, 1, 2,
            0, 2, 3
        });

        _vao.SetAttribPointer(0, 2, 4 * sizeof(float), IntPtr.Zero);
        _vao.SetAttribPointer(1, 2, 4 * sizeof(float), IntPtr.Zero + 2 * sizeof(float));
        OpenGLErrorChecker.CheckError();

        _textureId = GenTexture("./Assets/lost_empire-RGB.png");
        _textureAlphaChannelId = GenTexture("./Assets/lost_empire-Alpha.png");
        OpenGLErrorChecker.CheckError();

    }

    private double _totalTime = 0;
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        Title = $"{args.Time}ms";
        _totalTime += args.Time;
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ClearColor((float)Math.Sin(_totalTime), (float)Math.Cos(_totalTime), 0, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        _vao.Bind();
        
        _program.Use();
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, (TextureHandle)_textureId);
        _program.Upload("tex", 0);
        
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2d, (TextureHandle)_textureAlphaChannelId);
        _program.Upload("alphaChannel", 1);

        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
        base.OnRenderFrame(args);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (e.Width != 0 && e.Height != 0)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        base.OnResize(e);
    }

    private (int width, int height, byte[] data) LoadTexture(string filename)
    {
        var img = Image.Load<Rgb24>(filename);
        var pixels = new byte[img.Width * img.Height * 3];
        img.Frames.RootFrame.CopyPixelDataTo(pixels);
        return (img.Width, img.Height, pixels);
    }
    
    private TextureHandle GenTexture(string fileName)
    {
        var id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, id);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        fileName = fileName.Replace("\\", "/");
        (int width, int height, byte[] data) img;
        img = LoadTexture(fileName);
        GL.TexImage2D(TextureTarget.Texture2d, 0, (int)PixelFormat.Rgb, img.width, img.height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, img.data);
        return id;
    }
}