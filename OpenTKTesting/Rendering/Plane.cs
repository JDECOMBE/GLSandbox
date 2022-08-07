using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTKTesting.Rendering;

public class Plane : IRenderingItem
{
    private ShaderProgram _program;
    private VertexBufferObject<float> _vbo;
    private VertexArrayObject _vao;
    private ElementBufferObject _ebo;

    public Vector3 Position
    {
        set => _program.Upload("offset", value);
    }

    public Vector3 Color
    {
        set => _program.Upload("color", value);
    }

    public float Scale
    {
        set => _program.Upload("scale", value);
    }

    public void Init()
    {
        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "./Shaders/basic_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "./Shaders/basic_frag.glsl")
        });

        
        Color = Vector3.One;
        Scale = 1f;
        
        _vao = new VertexArrayObject();
        _vbo = new VertexBufferObject<float>(new float[]
        {
            -1, 0, -1,
            -1, 0, 1,
            1, 0, 1,
            1, 0, -1,
        });
        _ebo = new ElementBufferObject(new uint[]
        {
            0, 1, 2,
            0, 2, 3
        });

        _vao.SetAttribPointer(0, 3, 3 * sizeof(float), IntPtr.Zero);

        Console.WriteLine($"Plane VAO ({_vao.ID})");
        Console.WriteLine($"Plane VBO ({_vbo.ID})");
    }

    public void Render(Camera camera)
    {
        _vao.Bind();
        _program.Use();
        _program.Upload("viewProjection", camera.ViewProjection);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
    }

    public void Update(float dts) { }
}