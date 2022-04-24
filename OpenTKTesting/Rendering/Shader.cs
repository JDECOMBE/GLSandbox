using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTesting.Utils;

namespace OpenTKTesting.Rendering;

internal class Shader : IDisposable
{
    public readonly int ID;
    public readonly ShaderType ShaderType;
    public string FilePath { get; }

    public Shader(ShaderType shaderType, string filePath)
    {
        ShaderType = shaderType;
        FilePath = filePath;

        ID = GL.CreateShader(shaderType);
        GL.ShaderSource(ID, filePath.GetFileContent());
        GL.CompileShader(ID);

        string compileInfo = GL.GetShaderInfoLog(ID);
        if (compileInfo != string.Empty)
        {
            if (compileInfo.StartsWith("ERROR"))
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(compileInfo);
            Console.ResetColor();
        }
    }

    public void Dispose()
    {
        GL.DeleteShader(ID);
    }
}

class ShaderProgram : IDisposable
{
    private static int lastBindedID = -1;

    public int ID { get; private set; }
    private Dictionary<ShaderType, string> _shaderRefs = new Dictionary<ShaderType, string>();
    private List<FileSystemWatcher> _watchers = new();
    private bool _requestRebuildAtNextBind = false;
    private DateTime _lastWriteDateTime = DateTime.MinValue;

    public ShaderProgram(params Shader[] shaders)
    {
        foreach (var s in shaders)
        {
            _shaderRefs.Add(s.ShaderType, s.FilePath);
            var w = new FileSystemWatcher(Path.GetDirectoryName(s.FilePath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = Path.GetFileName(s.FilePath) ?? string.Empty
            };
            w.Changed += (_, e) =>
            {
                var lastWriteTime = File.GetLastWriteTime(s.FilePath);
                if (lastWriteTime.Minute != _lastWriteDateTime.Minute ||
                    lastWriteTime.Second != _lastWriteDateTime.Second)
                {
                    // Horrible I know, just a quick workaround coz this is made for debugging purposes and I really don't wanna spend time on debugging why this behaves weirdly... :) 
                    Task.Delay(500).Wait();
                    _lastWriteDateTime = lastWriteTime;
                    _requestRebuildAtNextBind = true;
                }
            };

            w.EnableRaisingEvents = true;
            _watchers.Add(w);
        }

        BuildShader(shaders);
    }

    private void BuildShader(params Shader[] shaders)
    {
        if (shaders is null || shaders.Length == 0 || shaders.Any(s => s.ID == 0))
            throw new IndexOutOfRangeException($"Shader array is empty or null. Or at least one shader has ID 0");

        if (!shaders.All(s => shaders.All(s1 => s.ID == s1.ID || s1.ShaderType != s.ShaderType)))
            throw new Exception($"A ShaderProgram can only hold one instance of every ShaderType. Validate the shader array.");

        ID = GL.CreateProgram();
        Console.WriteLine($"Building Shader {ID}");

        foreach (var s in shaders)
            GL.AttachShader(ID, s.ID);

        GL.LinkProgram(ID);
        foreach (var s in shaders)
        {
            GL.DetachShader(ID, s.ID);
            s.Dispose();
        }
    }

    private void RebuildShader()
    {
        var shaders = _shaderRefs.Select(e => new Shader(e.Key, e.Value)).ToArray();
        BuildShader(shaders);
    }

    public void Use()
    {
        if (_requestRebuildAtNextBind)
        {
            RebuildShader();
            _requestRebuildAtNextBind = false;
        }

        if (lastBindedID != ID)
        {
            GL.UseProgram(ID);
            lastBindedID = ID;
        }
    }

    public static void Use(int id)
    {
        if (lastBindedID != id)
        {
            GL.UseProgram(id);
            lastBindedID = id;
        }
    }

    public static void UploadToProgram(int id, int location, Matrix4 matrix4, bool transpose = false)
    {
        GL.ProgramUniformMatrix4(id, location, transpose, ref matrix4);
    }

    public void Upload(int location, Matrix4 matrix4, bool transpose = false)
    {
        GL.ProgramUniformMatrix4(ID, location, transpose, ref matrix4);
    }

    public void Upload(string name, Matrix4 matrix4, bool transpose = false)
    {
        GL.ProgramUniformMatrix4(ID, GetUniformLocation(name), transpose, ref matrix4);
    }

    public static void UploadToProgram(int id, int location, Vector4 vector4)
    {
        GL.ProgramUniform4(id, location, vector4);
    }

    public void Upload(int location, Vector4 vector4)
    {
        GL.ProgramUniform4(ID, location, vector4);
    }

    public void Upload(string name, Vector4 vector4)
    {
        GL.ProgramUniform4(ID, GetUniformLocation(name), vector4);
    }

    public static void UploadToProgram(int id, int location, Vector3 vector3)
    {
        GL.ProgramUniform3(id, location, vector3);
    }

    public void Upload(int location, Vector3 vector3)
    {
        GL.ProgramUniform3(ID, location, vector3);
    }

    public void Upload(string name, Vector3 vector3)
    {
        GL.ProgramUniform3(ID, GetUniformLocation(name), vector3);
    }

    public static void UploadToProgram(int id, int location, Vector2 vector2)
    {
        GL.ProgramUniform2(id, location, vector2);
    }

    public void Upload(int location, Vector2 vector2)
    {
        GL.ProgramUniform2(ID, location, vector2);
    }

    public void Upload(string name, Vector2 vector2)
    {
        GL.ProgramUniform2(ID, GetUniformLocation(name), vector2);
    }

    public static void UploadToProgram(int id, int location, float x)
    {
        GL.ProgramUniform1(id, location, x);
    }

    public void Upload(int location, float x)
    {
        GL.ProgramUniform1(ID, location, x);
    }

    public void Upload(string name, float x)
    {
        GL.ProgramUniform1(ID, GetUniformLocation(name), x);
    }

    public static void UploadToProgram(int id, int location, int x)
    {
        GL.ProgramUniform1(id, location, x);
    }

    public void Upload(int location, int x)
    {
        GL.ProgramUniform1(ID, location, x);
    }

    public void Upload(string name, int x)
    {
        GL.ProgramUniform1(ID, GetUniformLocation(name), x);
    }

    public static void UploadToProgram(int id, int location, uint x)
    {
        GL.ProgramUniform1(id, location, x);
    }

    public void Upload(int location, uint x)
    {
        GL.ProgramUniform1(ID, location, x);
    }

    public void Upload(string name, uint x)
    {
        GL.ProgramUniform1(ID, GetUniformLocation(name), x);
    }

    public static void UploadToProgram(int id, int location, bool x)
    {
        GL.ProgramUniform1(id, location, x ? 1 : 0);
    }

    public void Upload(int location, bool x)
    {
        GL.ProgramUniform1(ID, location, x ? 1 : 0);
    }

    public void Upload(string name, bool x)
    {
        GL.ProgramUniform1(ID, GetUniformLocation(name), x ? 1 : 0);
    }

    public int GetUniformLocation(string name)
    {
        return GL.GetUniformLocation(ID, name);
    }


    public void Dispose()
    {
        GL.DeleteProgram(ID);
    }
}