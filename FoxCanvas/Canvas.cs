using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace FoxCanvas;


/// <summary>
/// Вершина с текстурой для передачи в OpenGL.
/// </summary>
public struct Vertex
{
    /// <summary>
    /// Нормализованная координата вершины.
    /// </summary>
    public float NX, NY;

    /// <summary>
    /// Текстурная координата.
    /// </summary>
    public float TexX, TexY;
}


// TODO: проверка передаваемых значений у методов и свойств, установка начальных значений свойств.

/// <summary>
/// Холст для быстрого вывода массива пикселей на экран.
/// Работает в контексте OpenTK.
/// </summary>
public class Canvas : IDisposable
{
    /// <summary>
    /// Количество однобайтовых каналов на цвет.
    /// </summary>
    private const int CHANNEL_COUNT = 4;

    /// <summary>
    /// Ширина холста.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Высота холста.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Соотношение сторон холста.
    /// </summary>
    public float AspectRatio { get => (float)Width / Height; }

    /// <summary>
    /// Ширина области просмотра холста.
    /// </summary>
    public int ViewportWidth { get; private set; }

    /// <summary>
    /// Высота области просмотра холста.
    /// </summary>
    public int ViewportHeight { get; private set; }

    public float PixelSize {  get => (float)ViewportWidth / Width; }

    /// <summary>
    /// Размер данных в байтах нужный для хранения информации о изображении холста.
    /// </summary>
    private int DataSize { get => Width * Height * CHANNEL_COUNT; }

    /// <summary>
    /// Шейдерная программа OpenGL.
    /// </summary>
    private readonly Shader _shader;

    /// <summary>
    /// Текстура OpenGL для вывода изображения холста.
    /// </summary>
    private readonly Texture _texture;

    /// <summary>
    /// VAO OpenGL.
    /// </summary>
    private VAO _VAO;

    /// <summary>
    /// VBO OpenGL, хранит информацию о вершинах <see cref="_vertexes"/>.
    /// </summary>
    private VBO _VBO;

    /// <summary>
    /// Pixel Buffer Object для передачи изображения в текстуру.
    /// Пока один буффер используется для копирования пикселей с CPU на GPU,
    /// другой для копирования в текстурный объект GPU без участия CPU,
    /// далее меняются ролями.
    /// </summary>
    private VBO[] _PBOs = new VBO[2];

    /// <summary>
    /// Вершины для создания области холста в виде четырёхугольника.
    /// </summary>
    private Vertex[] _vertexes =
    {
        new() { NX =  1.0f, NY =  1.0f, TexX = 1.0f, TexY = 0.0f }, // top right
        new() { NX =  1.0f, NY = -1.0f, TexX = 1.0f, TexY = 1.0f }, // bottom right
        new() { NX = -1.0f, NY = -1.0f, TexX = 0.0f, TexY = 1.0f }, // bottom left
        new() { NX = -1.0f, NY =  1.0f, TexX = 0.0f, TexY = 0.0f }  // top left
    };

    private bool _disposed = false;


    /// <summary>
    /// Инициализировать холст с заданными размерами.
    /// </summary>
    /// <param name="width">Ширина холста.</param>
    /// <param name="height">Высота холста.</param>
    public Canvas(int width, int height, int viewportWidth, int viewportHeight)
    {
        Width = width;
        Height = height;

        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;

        _shader = new Shader(VERT_SHADER, FRAG_SHADER);

        int vertLocation = _shader.GetAttribLocation("aPosition");
        int texLocation = _shader.GetAttribLocation("aTexCoord");
        int stride = 4 * sizeof(float);
        int offsetVert = 0;
        int offsetTex = 2 * sizeof(float);

        _VBO = new(BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
        _VBO.SetData(_vertexes);

        _VAO = new(_vertexes.Length, PrimitiveType.TriangleFan);
        _VAO.AttachVBO(vertLocation, _VBO, 2, VertexAttribPointerType.Float, stride, offsetVert);
        _VAO.AttachVBO(texLocation, _VBO, 2, VertexAttribPointerType.Float, stride, offsetTex);

        _texture = new(Width, Height);

        _PBOs[0] = new(BufferTarget.PixelUnpackBuffer, BufferUsageHint.StreamDraw);
        _PBOs[0].ReserveMemory(DataSize);
        _PBOs[1] = new(BufferTarget.PixelUnpackBuffer, BufferUsageHint.StreamDraw);
        _PBOs[1].ReserveMemory(DataSize);
    }


    /// <summary>
    /// Задать массив цветов пикселей, который будет отображаться на холсте.
    /// </summary>
    /// <param name="image">Массив цветов для отображения.</param>
    public void SetImage(Color[,] image)
    {
        _texture.Use(TextureUnit.Texture0);
        _PBOs[0].Use();

        // Предотвращает ожидание отрисовки текстуры
        _PBOs[0].ReserveMemory(DataSize);

        IntPtr ptr = GL.MapBuffer(BufferTarget.PixelUnpackBuffer, BufferAccess.WriteOnly);

        if (ptr != IntPtr.Zero)
        {
            UpdatePBOData(ptr, image);
            GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);
        }

        // Копирование пикселей из PBO в текстуру
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, PixelFormat.AbgrExt, PixelType.UnsignedByte, 0);
    }


    /// <summary>
    /// Отобразить холст в окне OpenTK.
    /// </summary>
    public void Render()
    {
        _texture.Use(TextureUnit.Texture0);
        _shader.Activate();
        _VAO.Draw();
        _shader.Deactivate();
    }


    /// <summary>
    /// Задать область просмотра холста.
    /// </summary>
    /// <param name="width">Ширина области.</param>
    /// <param name="height">Высота области.</param>
    public void SetViewport(int width, int height)
    {
        ViewportWidth = width;
        ViewportHeight = height;

        if (width > height)
        {
            float newX = (float)height / width;

            _vertexes[0].NX = newX;
            _vertexes[1].NX = newX;
            _vertexes[2].NX = -newX;
            _vertexes[3].NX = -newX;

            _vertexes[0].NY = 1.0f;
            _vertexes[1].NY = -1.0f;
            _vertexes[2].NY = -1.0f;
            _vertexes[3].NY = 1.0f;
        }
        else if (width < height)
        {
            float newY = (float)width / height;

            _vertexes[0].NY = newY;
            _vertexes[1].NY = -newY;
            _vertexes[2].NY = -newY;
            _vertexes[3].NY = newY;

            _vertexes[0].NX = 1.0f;
            _vertexes[1].NX = 1.0f;
            _vertexes[2].NX = -1.0f;
            _vertexes[3].NX = -1.0f;
        }
  
        _VBO.UpdateData(_vertexes);
    }


    /// <summary>
    /// Конвертировать оконные координаты в координаты холста.
    /// </summary>
    /// <param name="x">Оконная координата X.</param>
    /// <param name="y">Оконная координата Y.</param>
    /// <returns>Координаты на холсте.</returns>
    public (int canvasX, int canvasY) GetCoord(float x, float y)
    {
        int canvasX = (int)Math.Floor(x / PixelSize);
        int canvasY = (int)Math.Floor(y / PixelSize);

        return (canvasX, canvasY);
    }


    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _texture.Dispose();
        _VBO.Dispose();
        _VAO.Dispose();
        _shader.Dispose();     
    }


    /// <summary>
    /// Обновить данные PBO массивом цветов пикселей.
    /// </summary>
    /// <param name="buffer">Указатель на PBO.</param>
    /// <param name="image">Массив пикселей для загрузки в PBO.</param>
    private unsafe void UpdatePBOData(IntPtr buffer, Color[,] image)
    {
        if (buffer == IntPtr.Zero)
            return;

        byte* ptr = (byte*)buffer.ToPointer();

        foreach (Color pixel in image)
        {
            *(ptr++) = pixel.A;
            *(ptr++) = pixel.B;
            *(ptr++) = pixel.G;
            *(ptr++) = pixel.R;
        }
    }


    /// <summary>
    /// Вертексный шейдер.
    /// </summary>
    private const string VERT_SHADER = @"
    #version 330 core

    in vec2 aPosition;
    in vec2 aTexCoord;

    out vec2 texCoord;

    void main(void)
    {
        texCoord = aTexCoord;

        gl_Position = vec4(aPosition, 1.0, 1.0);
    }
    ";


    /// <summary>
    /// Фрагментный шейдер.
    /// </summary>
    private const string FRAG_SHADER = @"
    #version 330

    out vec4 outputColor;

    in vec2 texCoord;

    uniform sampler2D texture0;

    void main()
    {
        outputColor = texture(texture0, texCoord);
    }
    ";
}
