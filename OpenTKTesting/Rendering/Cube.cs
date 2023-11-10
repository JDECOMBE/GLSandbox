using System.Security.Cryptography.X509Certificates;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTesting.Utils;

namespace OpenTKTesting.Rendering;

public class Cube : IRenderingItem
{
    private ShaderProgram _program;
    private VertexBufferObject<float> _vbo;
    private VertexArrayObject _vao;
    private ElementBufferObject _ebo;

    public Vector3 Position
    {
        set => _program.Upload("offset", value);
    }

    public void
        Init()
    {
        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "./Shaders/basic_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "./Shaders/basic_frag.glsl")
        });

        _vao = new VertexArrayObject();
        _vbo = new VertexBufferObject<float>(new float[]
        {
            -1, -1, -1,
            -1, 1, -1,
            -1, -1, 1,
            -1, 1, 1,

            1, -1, -1,
            1, 1, -1,
            1, -1, 1,
            1, 1, 1,
        });
        _ebo = new ElementBufferObject(new uint[]
        {
            1, 2, 0,
            1, 3, 2,
            5, 3, 1,
            5, 7, 3,
            4, 7, 5,
            4, 6, 7,
            0, 6, 4,
            0, 2, 6,
            3, 6, 2,
            3, 7, 6,
            5, 0, 4,
            5, 1, 0
        });


        _vao.SetAttribPointer(0, 3, 3 * sizeof(float), IntPtr.Zero);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        Console.WriteLine($"Loading VAO ({_vao.ID})");
        Console.WriteLine($"Loading VBO ({_vbo.ID})");
    }

    private float _currentTime = 0;
    public void Update(float dts)
    {
        _currentTime += dts;
    }

    public void Render(Camera camera, float dts = 0)
    {
        _vao.Bind();
        _program.Use();
        _program.Upload("viewProjection", camera.ViewProjection);
        _program.Upload("scale", 0.01f);
        _program.Upload("offset",  new Vector3((float)MathHelper.Sin(_currentTime / 2f) * 3f, (float)MathHelper.Cos(_currentTime / 2f) * 3f, 1f));
        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
    }
}
