using System.Drawing;
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
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _buffer[clickPos.X, clickPos.Y].Bg = DrawingBoard.PrimaryColor;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                _buffer[clickPos.X, clickPos.Y].Bg = DrawingBoard.SecondaryColor;
            }
        }

        base.OnMouseLeftDown(e);
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
