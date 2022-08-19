namespace OpenTKTesting.Rendering;

public interface IRenderingItem
{
    public void Init();
    public void Render(Camera camera, float dts = 0);
    public void Update(float dts);
}