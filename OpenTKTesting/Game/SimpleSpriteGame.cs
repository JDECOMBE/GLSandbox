using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTKTesting.Rendering;
using OpenTKTesting.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace OpenTKTesting.Game;

public class SimpleSpriteGame : GameWindow
{
    private ShaderProgram _program;
    private VertexArrayObject _vao;
    private int _textureId;

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

        _ = new VertexBufferObject<float>(new float[]
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

    }

    private double _totalTime = 0;
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        Title = $"{args.Time}ms";
        _totalTime += args.Time;
        GL.ClearColor(0, 0, 0, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        _vao.Bind();
        
        _program.Use();
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _textureId);
        _program.Upload("tex", 0);
        
        GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);
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
    
    private int GenTexture(string fileName)
    {
        var id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        fileName = fileName.Replace("\\", "/");
        (int width, int height, byte[] data) img;
        img = LoadTexture(fileName);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, img.width, img.height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, img.data);
        return id;
    }
}