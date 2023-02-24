using ConsoleGUI;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI;
using ConsoleGUI.UI.Events;

namespace WaveFunctionCollapse;

public class Canvas : Control
{
    public Canvas(Vector size)
    {
        _bufferSize = size;
        _buffer = new Pixel[size.X, size.Y];

        for (var x = 0; x < _bufferSize.X; x++)
        for (var y = 0; y < _bufferSize.Y; y++)
        {
            _buffer[x, y].Fg = Color.Empty;
            _buffer[x, y].Bg = Color.White;
        }

        ShowFocusedBorder = false;
    }

    public required DrawingBoard DrawingBoard { get; set; }
    private readonly Pixel[,] _buffer;
    private readonly Vector _bufferSize;

    public override void Resize()
    {
        MinSize = _bufferSize + InnerPadding * 2;

        ApplyResizing();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var clickPos = e.RelativeCursorPosition - InnerPadding;

        if (clickPos.X < _bufferSize.X && clickPos.Y < _bufferSize.Y &&
            clickPos is { X: >= 0, Y: >= 0 })
        {
            Draw(e, clickPos);
        }

        base.OnMouseLeftDown(e);
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
                _buffer[clickPos.X, clickPos.Y].Bg = color;
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
        var startColor = _buffer[startPos.X, startPos.Y].Bg;
        
        if (startColor == fillColor) return;
        
        var stack = new Stack<Vector>();
        stack.Push(startPos);

        while (stack.Count > 0)
        {
            var point = stack.Pop();
            
            if (_buffer[point.X, point.Y].Bg != startColor) continue;

            _buffer[point.X, point.Y].Bg = fillColor;
            
            if (point.X > 0) stack.Push(point with { X = point.X - 1 });
            if (point.X < _bufferSize.X - 1) stack.Push(point with { X = point.X + 1 });
            if (point.Y > 0) stack.Push(point with { Y = point.Y - 1 });
            if (point.Y < _bufferSize.Y - 1) stack.Push(point with { Y = point.Y + 1 });
        }
    }

    public override void Render()
    {
        base.Render();

        var pos = GlobalPosition + InnerPadding;

        // Display.DrawBuffer(pos, _buffer);

        for (var x = 0; x < _bufferSize.X; x++)
        for (var y = 0; y < _bufferSize.Y; y++)
        {
            Display.Draw(pos.X + x, pos.Y + y, ' ', Color.Empty, _buffer[x, y].Bg);
        }
    }
}
