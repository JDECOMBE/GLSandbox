using Dear_ImGui_Sample;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTKTesting.Rendering;
using OpenTKTesting.Utils;

namespace OpenTKTesting.Game;

public class BoidsSimulationGame : GameWindow
{
    #region FPS

    private int _frameCount = 0;
    private double _time = 0;

    #endregion

    #region ImGui

    private ImGuiController _controller;
    private bool _windowFocused = false;
    
    #endregion

    #region Config

    private float _maxSpeed = 2f;
    private float _maxForce = 0.005f;
    private float _alignmentFactor = 1f;
    private float _cohesionFactor = 1f;
    private float _separationFactor = 1f;
    

    #endregion
    private bool _pause = false;

    private Camera _camera;
    private Vector2? _previousMousePos;
    private BatchBoids _mesh;
    private BoundingBox _bb = new(Vector3.Zero, Vector3.One * 50, 50);

    private ShaderProgram _computeProgram;
    private ShaderStorageBufferObject _ssbo;

    public BoidsSimulationGame(GameWindowSettings settings, NativeWindowSettings nativeWindowSettings)
        : base(settings, nativeWindowSettings) { }

    public BoidsSimulationGame()
        : base(GameWindowSettings.Default, new NativeWindowSettings() {Size = new(800, 600), NumberOfSamples = 16}) { }

    private (Vector3[], Vector3[], Vector3[], Vector3[]) GenerateInstances(int count)
    {
        var pos = new Vector3[count];
        var colors = new Vector3[count];
        var velocity = new Vector3[count];
        var acceleration = new Vector3[count];

        var rand = new Random();

        for (int i = 0; i < count; i++)
        {
            pos[i] = new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle()) * 50f - Vector3.One * 25f; // <- [-25, 25] range
            colors[i] = new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle());
            velocity[i] = new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle()) * 2f - Vector3.One;
            acceleration[i] = Vector3.Zero;
        }

        return (pos, colors, velocity, acceleration);
    }

    private float[] InstancesToFloatArray(int count, Vector3[] positions, Vector3[] colors, Vector3[] velocity, Vector3[] acceleration)
    {
        return Enumerable.Range(0, count).SelectMany(e =>
        {
            return new[]
            {
                positions[e].X, positions[e].Y, positions[e].Z, 0f,
                colors[e].X, colors[e].Y, colors[e].Z, 0f,
                velocity[e].X, velocity[e].Y, velocity[e].Z, 0f,
                acceleration[e].X, acceleration[e].Y, acceleration[e].Z, 0f,
            };
        }).ToArray();
    }
    
    protected override void OnLoad()
    {
        
        _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        
        _camera = new Camera(Vector3.Zero, new Vector3(20, 0, 0), Size);

        const int nbOfInstances = 1000;
        var (pos, colors, velocity, acceleration) = GenerateInstances(nbOfInstances);
        var data = InstancesToFloatArray(nbOfInstances, pos, colors, velocity, acceleration);

        _ssbo = new ShaderStorageBufferObject();
        _ssbo.SetData(data);

        _computeProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, "../../../Shaders/boids.comp"));

        _mesh = new BatchBoids(@"./Assets/boid.fbx", nbOfInstances, _ssbo);
        _mesh.Init();

        _bb.Init();
        _bb.Color = new Vector4(.1f, .1f, 1f, 0.25f);
        VSync = VSyncMode.On;
        GL.Enable(EnableCap.DepthTest);
        base.OnLoad();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        
        _controller.Update(this, (float)args.Time);

        

        // Compute
        if (!_pause)
        {
            _computeProgram.Use();
            _computeProgram.Upload("maxSpeed", _maxSpeed);
            _computeProgram.Upload("maxForce", _maxForce);
            _computeProgram.Upload("alignmentFactor", _alignmentFactor);
            _computeProgram.Upload("cohesionFactor", _cohesionFactor);
            _computeProgram.Upload("separationFactor", _separationFactor);
            _ssbo.BindBuffer(2);

            _computeProgram.DispatchCompute(1000);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        }

        // Draw
        GL.ClearColor(0, 0, 0.1f, 0);
        GL.ClearDepth(1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _mesh.Pause = _pause;
        _mesh.Render(_camera, (float) args.Time);
        _bb.Render(_camera);

        ImGui.Begin("Config");
        _windowFocused = ImGui.IsWindowFocused();
        ImGui.DragFloat("Max Speed", ref _maxSpeed, 0.01f, 0, 2, "%.2f");
        ImGui.DragFloat("Max Force", ref _maxForce, 0.001f, 0, 1, "%.3f");
        ImGui.DragFloat("Alignment factor", ref _alignmentFactor, 0.01f, 0, 2, "%.2f");
        ImGui.DragFloat("Cohesion factor", ref _cohesionFactor, 0.01f, 0, 2, "%.2f");
        ImGui.DragFloat("Separation factor", ref _separationFactor, 0.01f, 0, 2, "%.2f");

        if (ImGui.Button("Reset configuration"))
        {
            _maxSpeed = 2f;
            _maxForce = 0.005f;
            _alignmentFactor = 1f;
            _cohesionFactor = 1f;
            _separationFactor = 1f;
        }
        ImGui.End();
        
        
        _controller.Render();

        _controller.WindowResized(ClientSize.X, ClientSize.Y);

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

    protected override void OnMaximized(MaximizedEventArgs e)
    {
        base.OnMaximized(e);

    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _previousMousePos = null;
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _controller.MouseScroll(e.Offset);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        _controller.PressChar((char)e.Unicode);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (IsKeyPressed(Keys.Escape))
            Close();

        _mesh.Update((float) args.Time);

        _frameCount++;
        _time += args.Time;

        if (_time > 1)
        {
            Title = $"FPS: {_frameCount / _time:F2} - {(_time / _frameCount):F4}ms";
            _time = 0;
            _frameCount = 0;
        }

        if (IsMouseButtonDown(MouseButton.Left) && !_windowFocused)
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

        if (IsKeyDown(Keys.W) || IsKeyDown(Keys.Z))
            _camera.Position += _camera.Direction * (float) args.Time * 2f;
        if (IsKeyDown(Keys.S))
            _camera.Position -= _camera.Direction * (float) args.Time * 2f;
        if (IsKeyPressed(Keys.Space))
            _pause = !_pause;

        base.OnUpdateFrame(args);
    }
}