﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FoxCanvas;

internal class Shader : IDisposable
{
    private const int INVALID_SHADER_PROGRAM = -1;

    private readonly int _vertexShader;
    private readonly int _fragmentShader;
    private int _shaderProgram;

    private bool _disposed = false;


    public Shader(string vertexShader, string fragmentShader)
    {
        _vertexShader = CreateShader(ShaderType.VertexShader, vertexShader);
        _fragmentShader = CreateShader(ShaderType.FragmentShader, fragmentShader);

        _shaderProgram = GL.CreateProgram();

        GL.AttachShader(_shaderProgram, _vertexShader);
        GL.AttachShader(_shaderProgram, _fragmentShader);

        GL.LinkProgram(_shaderProgram);

        GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(_shaderProgram);
            throw new Exception($"Shader program linking error ({infoLog}).");
        }

        DeleteShader(_vertexShader);
        DeleteShader(_fragmentShader);
    }


    public void Activate()
    {
        GL.UseProgram(_shaderProgram);
    }


    public void Deactivate()
    {
        GL.UseProgram(0);
    }


    public int GetAttribLocation(string name)
    {
        return GL.GetAttribLocation(_shaderProgram, name);
    }


    public void SetUniform4(string name, Vector4 vec)
    {
        int location = GL.GetUniformLocation(_shaderProgram, name);
        GL.Uniform4(location, vec);
    }


    private int CreateShader(ShaderType type, string source)
    {
        int id = GL.CreateShader(type);

        GL.ShaderSource(id, source);
        GL.CompileShader(id);

        GL.GetShader(id, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(id);
            throw new Exception($"Shader compilation error ({infoLog}).");
        }

        return id;
    }


    private int CreateShaderFromFile(ShaderType type, string path)
    {
        string source = File.ReadAllText(path);
        int id = CreateShader(type, source);

        return id;
    }


    private void DeleteShader(int shader)
    {
        GL.DetachShader(_shaderProgram, shader);
        GL.DeleteShader(shader);
    }


    private void ReleaseShaderProgram()
    {
        if (_shaderProgram == INVALID_SHADER_PROGRAM)
            return;

        GL.DeleteProgram(_shaderProgram);
        _shaderProgram = INVALID_SHADER_PROGRAM;
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
        ReleaseShaderProgram();
    }


    ~Shader()
    {
        InternalDispose();
    }
}
