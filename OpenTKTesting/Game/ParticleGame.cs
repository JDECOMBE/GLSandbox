using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTKTesting.Rendering;

namespace OpenTKTesting.Game;

public class ParticleGame : GameWindow
{
    #region FPS

    private int _frameCount = 0;
    private double _time = 0;

    #endregion

    private Camera _camera;
    private ParticuleSystem _particule = new ParticuleSystem();
    private Vector2? _previousMousePos;

    public ParticleGame(GameWindowSettings settings, NativeWindowSettings nativeWindowSettings)
        : base(settings, nativeWindowSettings)
    {
        
    }
    public ParticleGame()
        : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new (800, 600)})
    {
    }

    protected override void OnLoad()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("WARNING: This requires Opengl 4.3 to work as it is intended as a playground for Shader Storage Buffer Objects.");
        Console.ResetColor();
        _camera = new Camera(Vector3.Zero, new Vector3(1), Size);
        _particule.Init();
        _particule.CenterOfMass = Vector3.Zero;
        
        VSync = VSyncMode.On;
        base.OnLoad();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(0, 0, 0.1f, 0);
        GL.ClearDepth(1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _particule.Render(_camera);
        SwapBuffers();
        base.OnRenderFrame(args);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        if (e.Width != 0 && e.Height != 0)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _camera.UpdateSize(e.Size);
        }

        base.OnResize(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _previousMousePos = null;
        base.OnMouseUp(e);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (IsKeyPressed(Keys.Escape))
            Close();
        
        _particule.Update((float) args.Time);
        _frameCount++;
        _time += args.Time;

        if (_time > 1)
        {
            Title = $"FPS: {_frameCount / _time:F2}";
            _time = 0;
            _frameCount = 0;
        }

        _particule.Mass = IsMouseButtonDown(MouseButton.Right) ? 1 : 0;
        if (IsMouseButtonDown(MouseButton.Right))
        {
            var rayDir = _camera.GetWorldSpaceRay(MousePosition);
            var camDir = _camera.Direction;
            var dot = Vector3.Dot(camDir, rayDir);
            var dist = Vector3.Dot(_camera.Target - _camera.Position, camDir) / dot;
            _particule.CenterOfMass = _camera.Position + rayDir * dist;
        }
        
        if (IsMouseButtonDown(MouseButton.Left))
        {
            if (_previousMousePos.HasValue)
            {
                var delta = _previousMousePos.Value - MousePosition;
                delta = new Vector2(delta.X / Size.X, delta.Y / Size.Y) * (float) Math.PI;

                _camera.Position *= Matrix3.CreateRotationY(delta.X);
                var rotationAxis = Vector3.Cross(_camera.Direction, Vector3.UnitY);
                _camera.Position *= Matrix3.CreateFromAxisAngle(rotationAxis, delta.Y);
            }

            _previousMousePos = MousePosition;
        }


        if (IsKeyDown(Keys.W))
            _camera.Position += _camera.Direction * (float) args.Time * 2f;
        if (IsKeyDown(Keys.S))
            _camera.Position -= _camera.Direction * (float) args.Time * 2f;
        base.OnUpdateFrame(args);
    }
}