using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTesting.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace OpenTKTesting.Rendering;

public class BatchBillboard : IRenderingItem
{

    private ShaderProgram _program;
    private VertexArrayObject _vao;
    private VertexBufferObject<float> _instanceVbo;
    private int _indicesCount = 0;
    
    private float[] _data;

    private int _texture1;

    
    public int InstanceCount { get; set; }
    
    public BatchBillboard(int count, Vector3[] positions, Vector3[] colors, float[] scales, float[] randomValue)
    {
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
    
    public BatchBillboard(int count, VertexBufferObject<float> instanceVbo)
    {
        InstanceCount = count;
        _instanceVbo = instanceVbo;
    }
    
    private (int width, int height, byte[] data) LoadTexture(string filename)
    {
        var img = Image.Load<Rgba32>(filename);
        var pixels = new byte[img.Width * img.Height * 4];
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
        var img = LoadTexture(fileName);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.width, img.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.data);
        return id;
    }
    
    public void Init()
    {
        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "../../../Shaders/batch_billboard_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/batch_billboard_frag.glsl")
        });

        var vertices = new float[]
        {
            // Pos                                // Sampler    // Texture Coords  
             0f,          2f,          1f,        0,            0f, 0f,
             0f,          0f,          1f,        0,            0f, 1f,
             0f,          2f,         -1f,        0,            1f, 0f,
             0f,          0f,         -1f,        0,            1f, 1f,
                                      
             0.866025f,   2.000000f,  -0.500000f, 1,            0f, 0f,
             0.866025f,   0.000000f,  -0.500000f, 1,            0f, 1f,
            -0.866025f,   2.000000f,   0.500000f, 1,            1f, 0f,
            -0.866025f,   0.000000f,   0.500000f, 1,            1f, 1f,
            
            -0.866026f,   2.000000f,  -0.500000f, 2,            0f, 0f,
            -0.866025f,   0.000000f,  -0.500000f, 2,            0f, 1f,
             0.866025f,   2.000000f,   0.500000f, 2,            1f, 0f,
             0.866026f,   0.000000f,   0.500000f, 2,            1f, 1f,
        };

        var indices = new uint[]
        {
            0, 1, 2,
            1, 2, 3,
            
            4, 5, 6,
            5, 6, 7,
            
            8, 9, 10,
            9, 10, 11,
        };
        

        _vao = new VertexArrayObject();
        var vbo = new VertexBufferObject<float>(vertices);

        _instanceVbo ??= new VertexBufferObject<float>(_data);
        _indicesCount = indices.Length;
        _ = new ElementBufferObject(indices);
        vbo.Bind();

        _vao.SetAttribPointer(0, 3, 6 * sizeof(float), IntPtr.Zero);
        _vao.SetAttribPointer(1, 1, 6 * sizeof(float), IntPtr.Zero + 3 * sizeof(float));
        _vao.SetAttribPointer(2, 2, 6 * sizeof(float), IntPtr.Zero + 4 * sizeof(float));

        
        _instanceVbo.Bind();
        _vao.SetAttribPointer(3, 3, sizeof(float) * 8, IntPtr.Zero);
        _vao.SetAttribPointer(4, 3, sizeof(float) * 8, IntPtr.Zero + 3 * sizeof(float));
        _vao.SetAttribPointer(5, 1, sizeof(float) * 8, IntPtr.Zero + 6 * sizeof(float));
        _vao.SetAttribPointer(6, 1, sizeof(float) * 8, IntPtr.Zero + 7 * sizeof(float));
        GL.VertexAttribDivisor(3, 1);
        GL.VertexAttribDivisor(4, 1);
        GL.VertexAttribDivisor(5, 1);
        GL.VertexAttribDivisor(6, 1);


        _texture1 = GenTexture(@"./Assets/tree.png");
    }

    
    
    public void Render(Camera camera)
    {
        _vao.Bind();
        _program.Use();
        _program.Upload("viewProjection", camera.ViewProjection);
        _program.Upload("viewPosition", camera.Position);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture1);
        _program.Upload("texture1", 0);
        
        GL.DrawElementsInstanced(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero, InstanceCount);
    }

    public void Update(float dts) { }
}