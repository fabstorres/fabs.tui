namespace Fabs.Tui.Component;


public interface IComponent
{
    public string[] Render(int width);
    public void HandleInput(string data);
    public void Invalidate();
    public bool HasFocus { get; }
}
