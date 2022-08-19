using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTesting.Utils;

namespace OpenTKTesting.Rendering;

public class BoundingBox : IRenderingItem
{
    private ShaderProgram _program;
    private VertexArrayObject _vao;

    private float[] _data;

    public BoundingBox(Vector3 position, Vector3 size, int subdivisions)
    {
        Position = position;
        Size = size;

        GenerateData(subdivisions);
    }

    private void GenerateData(int subdivisions)
    {
        var data = new List<float>();

        var step = 1f / (subdivisions + 1);

        for (var i = 0; i < subdivisions + 2; i++)
        {
            var adv = i * step * 2 - 1;

            var x1 = new[]
            {
                adv, -1, -1,
                adv,  1, -1,
                adv, -1,  1,
                adv,  1,  1,
            };

            var x2 = new[]
            {
                adv, -1,  1,
                adv, -1, -1,
                adv,  1, -1,
                adv,  1,  1,
            };
            
            var y1 = new[]
            {
                -1, adv, -1,
                 1, adv, -1,
                -1, adv,  1,
                 1, adv,  1,
            };
            var y2 = new[]
            {
                -1, adv,  1,
                -1, adv, -1,
                 1, adv, -1,
                 1, adv,  1,
            };

            var z1 = new[]
            {
                -1, -1, adv,
                 1, -1, adv,
                -1,  1, adv,
                 1,  1, adv,
            };
            var z2 = new[]
            {
                -1,  1, adv,
                -1, -1, adv,
                 1, -1, adv,
                 1,  1, adv,
            };
                
            data.AddRange(x1);
            data.AddRange(x2);
            data.AddRange(y1);
            data.AddRange(y2);
            data.AddRange(z1);
            data.AddRange(z2);
        }

        _data = data.ToArray();
    }

    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }
    public Vector4 Color { get; set; }

    public void Init()
    {
        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "../../../Shaders/wireframe_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/wireframe_frag.glsl")
        });

        _vao = new VertexArrayObject();
        _ = new VertexBufferObject<float>(_data);
        _vao.SetAttribPointer(0, 3, 3 * sizeof(float), IntPtr.Zero);

        Console.WriteLine($"Loading VAO ({_vao.ID})");
    }

    public void Update(float dts) { }

    public void Render(Camera camera, float dts = 0)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _vao.Bind();
        _program.Use();
        
        _program.Upload("viewProjection", camera.ViewProjection);
        _program.Upload("scale", Size / 2f);
        _program.Upload("offset", Position);
        _program.Upload("color", Color);
        GL.DrawArrays(PrimitiveType.Lines, 0, _data.Length / 3);
        
        _vao.Unbind();
        ShaderProgram.Use(0);
        GL.Disable(EnableCap.Blend);

    }
}