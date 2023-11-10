using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace OpenTKTesting.Rendering;

public class BatchBoids : IRenderingItem
{

    private ShaderProgram _program;
    private VertexArrayObject _vao;
    private ShaderStorageBufferObject _dataBuffer;
    private ElementBufferObject _ebo;
    private int _indicesCount = 0;

    private string _filename = string.Empty;
    private float[] _data;

    public int InstanceCount { get; set; }
    public bool Pause { get; set; }

    public BatchBoids(string filename, int count, ShaderStorageBufferObject dataBuffer)
    {
        _filename = filename;
        InstanceCount = count;
        _dataBuffer = dataBuffer;
    }

    public BatchBoids(string fileName, int count, Vector3[] positions, Vector3[] colors, Vector3[] velocity, Vector3[] acceleration)
    {
        _filename = fileName;
        InstanceCount = count;

        _data = Enumerable.Range(0, count).SelectMany(e =>
        {
            return new[]
            {
                positions[e].X, positions[e].Y, positions[e].Z,
                colors[e].X, colors[e].Y, colors[e].Z,
                velocity[e].X, velocity[e].Y, velocity[e].Z,
                acceleration[e].X, acceleration[e].Y, acceleration[e].Z,
            };
        }).ToArray();
    }

    public void Init()
    {
        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "../../../Shaders/boids_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/boids_frag.glsl")
        });


        var assimpContext = new AssimpContext();
        var scene = assimpContext.ImportFile(_filename, PostProcessPreset.TargetRealTimeQuality | PostProcessSteps.FlipUVs);
        if (!scene.HasMeshes)
            throw new Exception("No Mesh contained");

        _vao = new VertexArrayObject();
        var vbo = new VertexBufferObject<float>(scene.Meshes[0].Vertices.SelectMany((v => new float[] {v.X, v.Y, v.Z})).ToArray());
        var normalVbo = new VertexBufferObject<float>(scene.Meshes[0].Normals.SelectMany(v => new float[] {v.X, v.Y, v.Z}).ToArray());

        if (_dataBuffer == null)
        {
            _dataBuffer = new ShaderStorageBufferObject();
            _dataBuffer.SetData(_data);
        }

        var indices = scene.Meshes[0].GetUnsignedIndices();
        _indicesCount = indices.Length;
        _ebo = new ElementBufferObject(indices);
        vbo.Bind();
        _vao.SetAttribPointer(0, 3, 3 * sizeof(float), IntPtr.Zero);

        normalVbo.Bind();
        _vao.SetAttribPointer(1, 3, 3 * sizeof(float), IntPtr.Zero);
    }

    public void Render(Camera camera, float dts)
    {
        _vao.Bind();
        _program.Use();

        _dataBuffer.BindBuffer(0);
        _program.Upload("viewProjection", camera.ViewProjection);
        _program.Upload("dts", dts);
        _program.Upload("pause", Pause);
        GL.DrawElementsInstanced(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero, InstanceCount);
        GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        _vao.Unbind();
        ShaderProgram.Use(0);
    }

    public void Update(float dts)
    {
    }
}
