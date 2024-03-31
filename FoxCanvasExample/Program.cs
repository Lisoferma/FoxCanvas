using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace FoxCanvasExample;

internal class Program
{
    static void Main()
    {
        NativeWindowSettings nativeWindowSettings = new()
        {
            Title = "Canvas Example",
            ClientSize = new Vector2i(600, 600),      
        };

        GameWindowSettings gameWindowSettings = new()
        {
            UpdateFrequency = 60.0
        };

        using (Window window = new(gameWindowSettings, nativeWindowSettings))
        {
            window.Run();
        }
    }
}
