using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTesting.Rendering;
using OpenTKTesting.Utils;

namespace OpenTKTesting.Game;

public class RayTracingGame : GameWindow
{

    private int _renderTextureId = -1;
    private ShaderProgram _computeProgram;
    private ShaderProgram _quadTextureProgram;
    private VertexArrayObject _vao;
    
    private const int WIDTH = 1920;
    private const int HEIGHT = 1080;
        

    public RayTracingGame(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        
    }

    public RayTracingGame() : base(GameWindowSettings.Default, new NativeWindowSettings { Size = new(WIDTH, HEIGHT) })
    {
        
    }

    private void LogComputeGroups()
    {
        GL.GetInteger((GetIndexedPName) All.MaxComputeWorkGroupCount, 0, out var c0);
        OpenGLErrorChecker.CheckError();
        GL.GetInteger((GetIndexedPName) All.MaxComputeWorkGroupCount, 1, out var c1);
        OpenGLErrorChecker.CheckError();
        GL.GetInteger((GetIndexedPName) All.MaxComputeWorkGroupCount, 2, out var c2);
        OpenGLErrorChecker.CheckError();
        GL.GetInteger((GetPName) All.MaxComputeWorkGroupInvocations, out var invocations);
        OpenGLErrorChecker.CheckError();

        Console.WriteLine($"Max Work Group => ({c0}, {c1}, {c2}) / MaxInvocations => {invocations}");
    }


    private void InitQuad()
    {

        _quadTextureProgram = new ShaderProgram(
            new Shader(ShaderType.VertexShader, "../../../Shaders/sprite_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/sprite_frag.glsl")
        );
        
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
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        LogComputeGroups();

        InitQuad();
        
        _renderTextureId = GenTexture(WIDTH, HEIGHT);
        GL.BindImageTexture(0, _renderTextureId, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

        _computeProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, "../../../Shaders/raytracing.comp"));
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        Title = $"{args.Time}ms";
        
        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BindImageTexture(0, _renderTextureId, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
        _computeProgram.Use();
        _computeProgram.DispatchCompute(WIDTH / 32 + 1, HEIGHT / 32 + 1);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        
        GL.ClearColor(1, 0, 0, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        _vao.Bind();
        _quadTextureProgram.Use();
        
        GL.BindTexture(TextureTarget.Texture2D, _renderTextureId);
        _quadTextureProgram.Upload("tex", 0);
        
        GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        
        SwapBuffers();

        base.OnRenderFrame(args);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
    }


    private int GenTexture(int width, int height)
    {
        var id = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, id);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        return id;
    }

}