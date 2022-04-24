using System.Drawing.Drawing2D;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace OpenTKTesting.Rendering;

class DepthRenderPass
{
    public FramebufferHandle FBO { get; private set; }
    public TextureHandle DepthTexture { get; private set; }

    public ShaderProgram Program { get; private set; }

    #region IRenderPass implementation

    public void Init()
    {
        FBO = GL.GenFramebuffer();

        DepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2d, DepthTexture);
        GL.TexImage2D(TextureTarget.Texture2d, 0, (int)PixelFormat.DepthComponent, 800, 600, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2d, DepthTexture, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferHandle.Zero);
        



        Program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "../../../Shaders/depth_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/depth_frag.glsl")
        });

    }

    public void PreRender()
    {
        GL.Viewport(0, 0, 800, 600);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.CullFace(CullFaceMode.FrontAndBack);
        Program.Use();
    }

    public void PostRender(Camera camera)
    {
        GL.Viewport(0, 0, (int) camera.WindowSize.X, (int) camera.WindowSize.Y);
        GL.CullFace(CullFaceMode.Back);

    }

    public void SetMatrix(Matrix4 viewProj)
    {
        Program.Upload("viewProjection", viewProj);
    }

    #endregion
}