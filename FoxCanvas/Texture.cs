using OpenTK.Graphics.OpenGL4;

namespace FoxCanvas;

public class Texture
{
    private const int INVALID_HANDLE = -1;

    public int Handle { get; private set; }

    private bool _disposed = false;


    public Texture(int width, int height)
    {
        Handle = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, Handle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    }


    public void Use(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }


    private void ReleaseHandle()
    {
        if (Handle == INVALID_HANDLE)
            return;

        GL.DeleteTexture(Handle);
        Handle = INVALID_HANDLE;
    }


    //public void Dispose()
    //{
    //    ReleaseHandle();
    //    GC.SuppressFinalize(this);
    //}


    //~Texture()
    //{
    //    //if (GraphicsContext.CurrentContext != null && !GraphicsContext.CurrentContext.IsDisposed)
    //    //ReleaseHandle();
    //    Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
    //}


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


    ~Texture()
    {
        InternalDispose();
    }
}
