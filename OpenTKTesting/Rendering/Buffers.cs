using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKTesting.Rendering;

public class VertexArrayObject
{
    public VertexArrayHandle ID { get; }

    public VertexArrayObject()
    {
        ID = GL.GenVertexArray();
        GL.BindVertexArray(ID);
    }

    public void SetAttribPointer(uint index, int size, int stride, IntPtr offset, bool normalize = false)
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
        GL.BindVertexArray(VertexArrayHandle.Zero);
    }
}

public class VertexBufferObject : Buffer<float>
{
    public VertexBufferObject(float[] data, BufferUsageARB usage = BufferUsageARB.StaticDraw) : base(BufferTargetARB.ArrayBuffer, data.Length, data, usage)
    {
    }
}

public class ElementBufferObject : Buffer<uint>
{
    public ElementBufferObject(uint[] data, BufferUsageARB usage = BufferUsageARB.StaticDraw) : base(BufferTargetARB.ElementArrayBuffer, data.Length, data, usage)
    {
    }
}

public class ShaderStorageBufferObject
{
    public BufferHandle ID { get; }

    public ShaderStorageBufferObject()
    {
        ID = GL.GenBuffer();
        BindBuffer();
        Console.WriteLine($"SSBO: {ID}");
    }

    public void BindBuffer()
    {
        GL.BindBuffer(BufferTargetARB.ShaderStorageBuffer, ID);
        GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, ID);
    }

    public unsafe void ImmutableAllocation<T>(int size, T[] data, BufferStorageMask bufferStorageFlags) where T : unmanaged
    {
        fixed (T* ptr = data)
            GL.NamedBufferStorage(ID, size, *ptr, bufferStorageFlags);
    }
}

public abstract class Buffer<T> where T : unmanaged
{
    public BufferHandle ID { get; }
    public BufferTargetARB Target { get; }

    protected unsafe Buffer(BufferTargetARB target, int bufferSize, T[] data, BufferUsageARB usage = BufferUsageARB.StaticDraw)
    {
        Target = target;
        ID = GL.GenBuffer();
        Bind();
        
        fixed (T* ptr = data)
            GL.BufferData(Target, bufferSize * System.Runtime.CompilerServices.Unsafe.SizeOf<T>(), *ptr, usage);
    }

    public void Bind()
    {
        GL.BindBuffer(Target, ID);
    }

    public void Unbind()
    {
        GL.BindBuffer(Target, BufferHandle.Zero);
    }
}