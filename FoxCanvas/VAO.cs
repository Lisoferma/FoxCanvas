using OpenTK.Graphics.OpenGL4;

namespace FoxCanvas;

public sealed class VAO : IDisposable
{
    private const int INVALID_HANDLE = -1;

    public int Handle { get; private set; }

    public int VertexCount { get; private set; }

    public PrimitiveType PrimitiveType { get; private set; }

    private bool _disposed = false;


    public VAO(int vertexCount, PrimitiveType primitiveType)
    {
        VertexCount = vertexCount;
        PrimitiveType = primitiveType;
        AcquireHandle();     
    }


    private void AcquireHandle()
    {
        Handle = GL.GenVertexArray();
    }


    public void Use()
    {
        GL.BindVertexArray(Handle);
    }


    public void AttachVBO(int index, VBO vbo, int elementsPerVertex, VertexAttribPointerType pointerType, int stride, int offset)
    {
        Use();
        vbo.Use();
        GL.EnableVertexAttribArray(index);
        GL.VertexAttribPointer(index, elementsPerVertex, pointerType, false, stride, offset);
    }


    public void Draw()
    {
        Use();
        GL.DrawArrays(PrimitiveType, 0, VertexCount);
    }


    private void ReleaseHandle()
    {
        if (Handle == INVALID_HANDLE)
            return;

        GL.DeleteVertexArray(Handle);
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


    ~VAO()
    {
        InternalDispose();
    }
}
