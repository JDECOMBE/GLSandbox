using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace OpenTKTesting.Rendering;

public class BatchMesh : IRenderingItem
{

    private ShaderProgram _program;
    private VertexArrayObject _vao;
    private VertexBufferObject<float> _instanceVbo;
    private ElementBufferObject _ebo;
    private int _indicesCount = 0;

    public VertexBufferObject<float> InstanceVBO => _instanceVbo;
    
    private string _filename = string.Empty;
    private float[] _data;

    public int InstanceCount { get; set; }


    public BatchMesh(string filename, int count, Buffer<float> instanceVbo)
    {
        _filename = filename;
        InstanceCount = count;
        _instanceVbo = instanceVbo as VertexBufferObject<float>;
    }
    
    public BatchMesh(string fileName, int count, Vector3[] positions, Vector3[] colors, float[] scales, float[] randomValue)
    {
        _filename = fileName;
        InstanceCount = count;

        _data = Enumerable.Range(0, count).SelectMany(e =>
        {
            return new float[]
            {
                positions[e].X, positions[e].Y, positions[e].Z,
                colors[e].X, colors[e].Y, colors[e].Z,
                scales[e], randomValue[e]
            };
        }).ToArray();
    }

    public void Init()
    {
        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "../../../Shaders/batch_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/batch_frag.glsl")
        });

        
        var assimpContext = new AssimpContext();
        var scene = assimpContext.ImportFile(_filename, PostProcessPreset.TargetRealTimeQuality | PostProcessSteps.FlipUVs);
        if (!scene.HasMeshes)
            throw new Exception("No Mesh contained");
        
        _vao = new VertexArrayObject();
        var vbo = new VertexBufferObject<float>(scene.Meshes[0].Vertices.SelectMany((v => new float[] {v.X, v.Y, v.Z})).ToArray());
        var normalVbo = new VertexBufferObject<float>(scene.Meshes[0].Normals.SelectMany(v => new float[] {v.X, v.Y, v.Z}).ToArray());
        _instanceVbo ??= new VertexBufferObject<float>(_data);
        var indices = scene.Meshes[0].GetUnsignedIndices();
        _indicesCount = indices.Length;
        _ebo = new ElementBufferObject(indices);
        vbo.Bind();
        _vao.SetAttribPointer(0, 3, 3 * sizeof(float), IntPtr.Zero);
        
        normalVbo.Bind();
        _vao.SetAttribPointer(1, 3, 3 * sizeof(float), IntPtr.Zero);
        
        _instanceVbo.Bind();
        _vao.SetAttribPointer(2, 3, sizeof(float) * 9, IntPtr.Zero);
        _vao.SetAttribPointer(3, 3, sizeof(float) * 9, IntPtr.Zero + 3 * sizeof(float));
        _vao.SetAttribPointer(4, 1, sizeof(float) * 9, IntPtr.Zero + 6 * sizeof(float));
        _vao.SetAttribPointer(5, 1, sizeof(float) * 9, IntPtr.Zero + 7 * sizeof(float));
        _vao.SetAttribPointer(6, 1, sizeof(float) * 9, IntPtr.Zero + 8 * sizeof(float));
        GL.VertexAttribDivisor(2, 1);
        GL.VertexAttribDivisor(3, 1);
        GL.VertexAttribDivisor(4, 1);
        GL.VertexAttribDivisor(5, 1);
        GL.VertexAttribDivisor(6, 1);
    }

    public void Render(Camera camera)
    {
        _vao.Bind();
        _program.Use();
        _program.Upload("viewProjection", camera.ViewProjection);
        _program.Upload("viewPosition", camera.Position);
        _program.Upload("windowSize", camera.WindowSize);
        GL.DrawElementsInstanced(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero, InstanceCount);
    }

    public void Update(float dts)
    {
    }
}