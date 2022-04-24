using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKTesting.Utils;

public static class OpenGLErrorChecker
{
    public static void CheckError()
    {
        var status = GL.GetError();
        if (status != ErrorCode.NoError)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(status);
            Console.ResetColor();
        }
    }
}