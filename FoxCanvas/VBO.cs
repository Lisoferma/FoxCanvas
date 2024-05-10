using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace FoxCanvas;

public sealed class VBO : IDisposable
{
    private const int INVALID_HANDLE = -1;

    public int Handle { get; private set; }

    public BufferTarget Type { get; private set; }

    public BufferUsageHint UsageHint { get; private set; }

    private bool _disposed = false;


    public VBO(BufferTarget type = BufferTarget.PixelUnpackBuffer, BufferUsageHint usageHint = BufferUsageHint.StreamDraw)
    {
        Type = type;
        UsageHint = usageHint;
        AcquireHandle();
    }


    private void AcquireHandle()
    {
        Handle = GL.GenBuffer();
    }


    public void Use()
    {
        GL.BindBuffer(Type, Handle);
    }


    public void SetData<T>(T[] data) where T : struct
    {
        if (data.Length == 0)
            throw new ArgumentException("The array must contain at least one element.", nameof(data));

        Use();
        GL.BufferData(Type, data.Length * Marshal.SizeOf(typeof(T)), data, UsageHint);
    }


    public void UpdateData<T>(T[] data) where T : struct
    {
        if (data.Length == 0)
            throw new ArgumentException("The array must contain at least one element.", nameof(data));

        Use();
        GL.BufferSubData(Type, (IntPtr)0, data.Length * Marshal.SizeOf(typeof(T)), data);
    }


    public void ReserveMemory(int size)
    {
        Use();
        GL.BufferData(Type, size, 0, UsageHint);
    }


    private void ReleaseHandle()
    {
        if (Handle == INVALID_HANDLE)
            return;

        GL.DeleteBuffer(Handle);
        Handle = INVALID_HANDLE;
    }


    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        InternalDispose();
        GC.SuppressFinalize(this);
    }


    private void InternalDispose()
    {
        ReleaseHandle();
    }


    ~VBO()
    {
        InternalDispose();
    }
}
