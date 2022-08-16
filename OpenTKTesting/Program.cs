using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTKTesting.Game;

var windowsSettings = NativeWindowSettings.Default;

#if DEBUG
windowsSettings.Size = new Vector2i(1920, 1080);
#else
windowsSettings.Size = new Vector2i(1920, 1080);
// windowsSettings.WindowState = WindowState.Fullscreen;
#endif

if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    windowsSettings.Flags = ContextFlags.ForwardCompatible;
}

#if DEBUG
var game = new BatchrenderingGame(GameWindowSettings.Default, windowsSettings);
#elif BATCHRENDERING
var game = new BatchrenderingGame(GameWindowSettings.Default, windowsSettings);
#elif  PARTICLESYSTEM
var game = new ParticleGame(GameWindowSettings.Default, windowsSettings);
#elif RAYTRACING
var game = new RayTracingGame(GameWindowSettings.Default, windowsSettings);
#elif SCENERENDERER
var game = new SceneRendererGame(GameWindowSettings.Default, windowsSettings);
#else
var game = new BatchrenderingGame(GameWindowSettings.Default, windowsSettings);
#endif


game.Run();