using System.Runtime.InteropServices;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTKTesting.Game;

var windowsSettings = NativeWindowSettings.Default;
if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    windowsSettings.Flags = ContextFlags.ForwardCompatible;
}

// var game = new ParticleGame(GameWindowSettings.Default, windowsSettings);
// var game = new SceneRendererGame(GameWindowSettings.Default, windowsSettings);
var game = new SimpleSpriteGame(GameWindowSettings.Default, windowsSettings);
game.Run();