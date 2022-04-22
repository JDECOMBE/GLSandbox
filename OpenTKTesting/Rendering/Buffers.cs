using OpenTK.Graphics.OpenGL4;

namespace OpenTKTesting.Rendering;

public class VertexArrayObject
{
    public int ID { get; }

    public VertexArrayObject()
    {
        ID = GL.GenVertexArray();
        GL.BindVertexArray(ID);
    }

    public void SetAttribPointer(int index, int size, int stride, IntPtr offset, bool normalize = false)
    {
        GL.VertexAttribPointer(index, size, VertexAttribPointerType.Float, normalize, stride, offset);
        GL.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        GL.BindVertexArray(ID);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }
}

public class VertexBufferObject : Buffer<float>
{
    public VertexBufferObject(float[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw) : base(BufferTarget.ArrayBuffer, data.Length, data, usage)
    {
    }
}

public class ElementBufferObject : Buffer<uint>
{
    public ElementBufferObject(uint[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw) : base(BufferTarget.ElementArrayBuffer, data.Length, data, usage)
    {
    }
}

public class ShaderStorageBufferObject
{
    public int ID { get; }

    public ShaderStorageBufferObject()
    {
        ID = GL.GenBuffer();
        BindBuffer();
        Console.WriteLine($"SSBO: {ID}");
    }

    public void BindBuffer()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ID);
    }

    public void ImmutableAllocation<T>(int size, T[] data, BufferStorageFlags bufferStorageFlags) where T : struct
    {
        GL.NamedBufferStorage(ID, size, data, bufferStorageFlags);
    }
}

public abstract class Buffer<T> where T : struct
{
    public int ID { get; }
    public BufferTarget Target { get; }

    protected Buffer(BufferTarget target, int bufferSize, T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        Target = target;
        ID = GL.GenBuffer();
        Bind();
        GL.BufferData(Target, bufferSize * System.Runtime.CompilerServices.Unsafe.SizeOf<T>(), data, usage);
    }

    public virtual void Bind()
    {
        GL.BindBuffer(Target, ID);
    }

    public virtual void Unbind()
    {
        GL.BindBuffer(Target, 0);
    }
}