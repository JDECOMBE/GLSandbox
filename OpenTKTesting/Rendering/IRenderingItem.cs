namespace OpenTKTesting.Rendering;

public interface IRenderingItem
{
    public void Init();
    public void Render(Camera camera);
    public void Update(float dts);
}