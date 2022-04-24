using System.Drawing.Drawing2D;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace OpenTKTesting.Rendering;

class DepthRenderPass
{
    public int FBO { get; private set; }
    public int DepthTexture { get; private set; }

    public ShaderProgram Program { get; private set; }

    #region IRenderPass implementation

    public void Init()
    {
        FBO = GL.GenFramebuffer();

        DepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 800, 600, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthTexture, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        



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