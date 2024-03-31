using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using FoxCanvas;

namespace FoxCanvasExample;

/// <summary>
/// Пример программы использующей FoxCanvas.
/// </summary>
internal class Window : GameWindow
{
    private const string TITLE = "Canvas Example";
    private const int CANVAS_WIDTH = 16;
    private const int CANVAS_HEIGHT = 16;

    private float _frameTime = 0.0f;
    private int _fps = 0;

    private Canvas _canvas;
    private Color[,] _image;


    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    { }


    protected override void OnLoad()
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        _image = new Color[CANVAS_WIDTH, CANVAS_HEIGHT];
        CreateGridImage(_image);

        _canvas = new Canvas(CANVAS_WIDTH, CANVAS_HEIGHT);
        _canvas.SetImage(_image);

        base.OnLoad();
    }


    protected override void OnRenderFrame(FrameEventArgs e)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        _canvas.Render();
        SwapBuffers();

        base.OnRenderFrame(e);
    }


    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        _frameTime += (float)e.Time;
        _fps++;

        if (_frameTime >= 1.0f)
        {
            Title = $"{TITLE} | {_fps} fps";
            _frameTime = 0.0f;
            _fps = 0;
        }

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        base.OnUpdateFrame(e);
    }


    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        _canvas.Viewport(e.Width, e.Height);
        base.OnFramebufferResize(e);       
    }


    protected override void OnUnload()
    {
        _canvas.Dispose();
        base.OnUnload();
    }


    private static void CreateGridImage(Color[,] image)
    {
        int rows = image.GetLength(0);
        int cols = image.GetLength(1);      

        bool setColor = false;

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                if (setColor)
                    image[i, j] = Color.Gray;
                else
                    image[i, j] = Color.DarkGray;

                setColor = setColor == false;
            }

            setColor = setColor == false;
        }
    }


    //protected override void OnMouseDown(MouseButtonEventArgs e)
    //{
    //    base.OnMouseDown(e);

    //    if (e.Button != MouseButton.Left)
    //        return;

    //    Vector2 mousePos = MousePosition;

    //    int x = (mousePos.X - Width / 2f) / (Width / 2f);
    //    int y = -(mousePos.Y - Height / 2f) / (Height / 2f);

    //    Console.WriteLine(x + " " + y);
    //}
}
