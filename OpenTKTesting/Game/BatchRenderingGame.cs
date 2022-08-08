using System.Net;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTKTesting.Rendering;
using OpenTKTesting.Utils;

namespace OpenTKTesting.Game;

public class BatchrenderingGame : GameWindow
{
    #region FPS

    private int _frameCount = 0;
    private double _time = 0;

    #endregion

    private Camera _camera;
    private Vector2? _previousMousePos;
    private Plane _plane = new Plane();
    private BatchMesh _mesh;
    private BatchBillboard _billboard;

    private ShaderProgram _computeProgram;
    private ShaderStorageBufferObject _ssbo;
    private ShaderStorageBufferObject _meshesData;
    private ShaderStorageBufferObject _billboardData;
    private ShaderStorageBufferObject _statsData;

    public BatchrenderingGame(GameWindowSettings settings, NativeWindowSettings nativeWindowSettings)
        : base(settings, nativeWindowSettings) { }

    public BatchrenderingGame()
        : base(GameWindowSettings.Default, new NativeWindowSettings() {Size = new(800, 600)}) { }

    private (Vector3[], Vector3[], float[], float[]) GenerateInstances(int count)
    {
        var pos = new Vector3[count];
        var colors = new Vector3[count];
        var scales = new float[count];
        var rd = new float[count];

        var rand = new Random();

        for (int i = 0; i < count; i++)
        {
            pos[i] = new Vector3(rand.NextSingle(), 0f, rand.NextSingle()) * 40f - new Vector3(20f, 0f, 20f);
            colors[i] = new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle());
            scales[i] = rand.NextSingle() * 0.3f + .7f;
            rd[i] = rand.NextSingle();
        }

        return (pos, colors, scales, rd);
    }

    private float[] InstancesToFloatArray(int count, Vector3[] positions, Vector3[] colors, float[] scales, float[] randomValue)
    {
        return Enumerable.Range(0, count).SelectMany(e =>
        {
            return new float[]
            {
                positions[e].X, positions[e].Y, positions[e].Z,
                colors[e].X, colors[e].Y, colors[e].Z,
                scales[e], randomValue[e]
            };
        }).ToArray();
    }

    protected override void OnLoad()
    {
        _camera = new Camera(Vector3.Zero, new Vector3(5), Size);

        _plane.Init();
        _plane.Scale = 20f;
        _plane.Color = Vector3.One * 0.6f;

        const int nbOfInstances = 10000;
        var (pos, colors, scales, rd) = GenerateInstances(nbOfInstances);
        var data = InstancesToFloatArray(nbOfInstances, pos, colors, scales, rd);

        // Input data of compute shader
        _ssbo = new ShaderStorageBufferObject();
        _ssbo.SetData(data);

        // Output of compute shader -> per instance data used as VBO
        var blankData = Enumerable.Range(0, sizeof(float) * 8 * nbOfInstances).Select(e => 0f).ToArray();
        _meshesData = new ShaderStorageBufferObject();
        _meshesData.SetData(blankData);
        
        _billboardData = new ShaderStorageBufferObject();
        _billboardData.SetData(blankData);

        _statsData = new ShaderStorageBufferObject();
        _statsData.SetData(new int[2] { 0, 0});
        
        _computeProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, "../../../Shaders/batch_filtering.comp"));

        OpenGLErrorChecker.CheckError();

        _billboard = new BatchBillboard(nbOfInstances, new VertexBufferObject<float>(_billboardData.ID));
        OpenGLErrorChecker.CheckError();

        _billboard.Init();

        _mesh = new BatchMesh(@"./Assets/tree.gltf", nbOfInstances, new VertexBufferObject<float>(_meshesData.ID));
        _mesh.Init();


        VSync = VSyncMode.Off;
        GL.Enable(EnableCap.DepthTest);
        base.OnLoad();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        // Compute
        _computeProgram.Use();
        _statsData.SetData(new int[2] { 0, 0});

        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, _ssbo.ID);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, _billboardData.ID);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, _meshesData.ID);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, _statsData.ID);
        GL.Uniform3(1, _camera.Position.X, _camera.Position.Y, _camera.Position.Z);

        _computeProgram.DispatchCompute(10000);
        GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        

        var data = new int[2];
        GL.GetNamedBufferSubData(_statsData.ID, IntPtr.Zero, sizeof(int) * 2, data);
        _mesh.InstanceCount = data[0];
        _billboard.InstanceCount = data[1];
        
        // Draw
        GL.ClearColor(0, 0, 0.1f, 0);
        GL.ClearDepth(1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _plane.Render(_camera);
        _mesh.Render(_camera);
        _billboard.Render(_camera);
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

        _plane.Update((float) args.Time);
        _mesh.Update((float) args.Time);
        _billboard.Update((float) args.Time);

        _frameCount++;
        _time += args.Time;

        if (_time > 1)
        {
            Title = $"FPS: {_frameCount / _time:F2}";
            _time = 0;
            _frameCount = 0;
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

        if (IsKeyDown(Keys.W) || IsKeyDown(Keys.Z))
            _camera.Position += _camera.Direction * (float) args.Time * 2f;
        if (IsKeyDown(Keys.S))
            _camera.Position -= _camera.Direction * (float) args.Time * 2f;
        base.OnUpdateFrame(args);
    }
}