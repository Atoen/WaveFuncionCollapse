using ConsoleGUI;
using ConsoleGUI.UI.Events;
using ConsoleGUI.UI.Widgets;

namespace WaveFunctionCollapse.UI;

public class InputCanvas : Canvas
{
    public InputCanvas(int width, int height) : base(width, height)
    {
    }

    public required DrawingBoard DrawingBoard { get; set; }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!InResizeMode) Draw(e);

        base.OnMouseMove(e);
    }

    private void Draw(MouseEventArgs e)
    {
        var clickPos = e.RelativeCursorPosition - InnerPadding;

        if (clickPos.X < Buffer.Size.X && clickPos.Y < Buffer.Size.Y &&
            clickPos is {X: >= 0, Y: >= 0})
        {
            Draw(e, clickPos);
        }
    }

    private void Draw(MouseEventArgs e, Vector clickPos)
    {
        Color color;

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            color = DrawingBoard.PrimaryColor;
        }

        else if (e.RightButton == MouseButtonState.Pressed)
        {
            color = DrawingBoard.SecondaryColor;
        }

        else return;

        switch (DrawingBoard.DrawMode)
        {
            case DrawMode.Pen:
                Buffer[clickPos].Bg = color;
                return;

            case DrawMode.Fill:
                Fill(clickPos, color);
                break;

            default:
                return;
        }
    }

    private void Fill(Vector startPos, Color fillColor)
    {
        var startColor = Buffer[startPos.X, startPos.Y].Bg;

        if (startColor == fillColor) return;

        var stack = new Stack<Vector>();
        stack.Push(startPos);

        while (stack.Count > 0)
        {
            var point = stack.Pop();

            if (Buffer[point.X, point.Y].Bg != startColor) continue;

            Buffer[point].Bg = fillColor;

            if (point.X > 0) stack.Push(point with { X = point.X - 1 });
            if (point.X < Buffer.Size.X - 1) stack.Push(point with { X = point.X + 1 });
            if (point.Y > 0) stack.Push(point with { Y = point.Y - 1 });
            if (point.Y < Buffer.Size.Y - 1) stack.Push(point with { Y = point.Y + 1 });
        }
    }
}