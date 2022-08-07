using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTesting.Utils;

namespace OpenTKTesting.Rendering;

public struct Particle
{
    public Vector3 Position { get; set; }
    private int pad1;
    
    public Vector3 Velocity { get; set; }
    private int pad3;
}

public class ParticuleSystem : IRenderingItem
{
    private ShaderStorageBufferObject _ssbo;
    private ShaderProgram _program;
    private const int PARTICLE_COUNT = 10_000_000;
    public Particle[] Particles = new Particle[PARTICLE_COUNT];
    
    private VertexArrayObject _vao;

    public Vector3 CenterOfMass
    {
        set => _program.Upload("centerOfMass", value);
    }

    public float Mass
    {
        set => _program.Upload("mass", value);
    }
    
    public void Init()
    {
        _program = new ShaderProgram(
            new Shader(ShaderType.VertexShader, "../../../Shaders/particle_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/particle_frag.glsl")
        );

        var r = new Random();

        for (int i = 0; i < PARTICLE_COUNT; i++)
        {
            var pos = new Vector3(r.NextSingle(), r.NextSingle(), r.NextSingle()) ;
            
            Particles[i] = new Particle()
            {
                Velocity = Vector3.Zero,
                Position = pos * 10 - Vector3.One * 5
            };
        }

        _vao = new VertexArrayObject();
        _ssbo = new ShaderStorageBufferObject();
        _ssbo.ImmutableAllocation(System.Runtime.CompilerServices.Unsafe.SizeOf<Particle>() * Particles.Length, Particles, BufferStorageFlags.DynamicStorageBit);
    }

    public void Update(float dts)
    {
        _program.Upload("dts", dts);
    }


    public void Render(Camera camera)
    {
        _vao.Bind();
        _program.Use();
        _program.Upload("projViewMatrix", camera.ViewProjection);
        GL.DrawArrays(PrimitiveType.Points, 0, Particles.Length);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
    }
}