using System.ComponentModel.DataAnnotations.Schema;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace OpenTKTesting.Rendering;

public class Mesh : IRenderingItem
{
    private ShaderProgram _program;
    private VertexBufferObject<float> _vbo;
    private VertexArrayObject _vao;
    private ElementBufferObject _ebo;
    private string _filename = string.Empty;
    private int _indicesCount = 0;

    public Mesh(string fileName)
    {
        _filename = fileName;
    }

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

        
        var assimpContext = new AssimpContext();
        var scene = assimpContext.ImportFile(_filename, PostProcessPreset.TargetRealTimeQuality | PostProcessSteps.FlipUVs);
        if (!scene.HasMeshes)
            throw new Exception("No Mesh contained");
        
        Color = Vector3.One;
        Scale = 1f;
        
        _vao = new VertexArrayObject();
        _vbo = new VertexBufferObject<float>(scene.Meshes[0].Vertices.SelectMany((v => new float[] {v.X, v.Y, v.Z})).ToArray());
        var indices = scene.Meshes[0].GetUnsignedIndices();
        _indicesCount = indices.Length;
        _ebo = new ElementBufferObject(indices);

        _vao.SetAttribPointer(0, 3, 3 * sizeof(float), IntPtr.Zero);

        Console.WriteLine($"Plane VAO ({_vao.ID})");
        Console.WriteLine($"Plane VBO ({_vbo.ID})");
    }

    public void Render(Camera camera)
    {
        _vao.Bind();
        _program.Use();
        _program.Upload("viewProjection", camera.ViewProjection);
        GL.DrawElements(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Update(float dts) { }
}