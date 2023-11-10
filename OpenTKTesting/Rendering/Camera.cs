using System.Numerics;
using OpenTK.Mathematics;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace OpenTKTesting.Rendering;

public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }

    public Vector3 Direction => (Target - Position).Normalized();

    public Vector2 WindowSize { get; private set; }
    public Matrix4 Projection { get; private set; }

    public Matrix4 View => Matrix4.LookAt(Position, Target, Vector3.UnitY);
    public Matrix4 ViewProjection => View * Projection;


    public Camera(Vector3 target, Vector3 position, Vector2 windowSize)
    {
        Target = target;
        Position = position;
        UpdateSize(windowSize);
    }

    public void UpdateSize(Vector2 windowSize)
    {
        WindowSize = windowSize;
        Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), windowSize.X / windowSize.Y, 0.01f, 1000f);
    }

    public Vector3 GetWorldSpaceRay(Vector2 mousePos)
    {
        mousePos.Y = WindowSize.Y - mousePos.Y; // [0, Width][0, Height]
        var normalizedDeviceCoords = Vector2.Divide(new Vector2(mousePos.X, mousePos.Y), WindowSize) * 2.0f - new Vector2(1.0f); // [-1.0, 1.0][-1.0, 1.0]
        var rayEye = new Vector4(normalizedDeviceCoords.X, normalizedDeviceCoords.Y, -1.0f, 1.0f) * Projection.Inverted();
        rayEye.Z = -1.0f;
        rayEye.W = 0.0f;
        return (rayEye * View.Inverted()).Xyz.Normalized();
    }

}
