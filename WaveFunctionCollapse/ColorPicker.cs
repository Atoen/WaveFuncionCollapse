using System.Drawing;
using ConsoleGUI;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI;
using ConsoleGUI.UI.Events;

namespace WaveFunctionCollapse;

public class ColorPicker : Control
{
    public ColorPicker() => ShowFocusedBorder = false;
    public List<Color> Colors { get; set; } = new();
    public Vector ColorCellSize { get; set; } = new(2, 2);
    public int ColorRows { get; set; } = 2;
    public Color PrimaryColor { get; private set; } = Color.Black;
    public Color SecondaryColor { get; private set; } = Color.White;

    public required DrawingBoard DrawingBoard { get; set; }

    private record struct ColorCell(Color Color, Vector Size, Vector Position);

    private readonly List<ColorCell> _colorCells = new();
    private Vector _pickerSize;
    private int _colorColumns;

    private void CalculateColorCellsPosition()
    {
        _colorCells.Clear();
        _colorColumns = (Colors.Count + ColorRows - 1) / ColorRows;

        var pos = new Vector(0, -ColorCellSize.Y);
        var i = 0;

        foreach (var color in Colors)
        {
            if (i % _colorColumns == 0)
            {
                pos.Y += ColorCellSize.Y;
                pos.X -= ColorCellSize.X * i;
                i = 0;
            }

            _colorCells.Add(new ColorCell(color, ColorCellSize, pos));
            pos.X += ColorCellSize.X;

            i++;
        }
    }

    private void SelectColor(Vector pickerClickPos, ColorType type)
    {
        var cellColumn = pickerClickPos.X / ColorCellSize.X;
        var cellRow = pickerClickPos.Y / ColorCellSize.Y;

        var cellIndex = cellRow * _colorColumns + cellColumn;

        if (cellIndex >= _colorCells.Count) return;

        var selectedColor = _colorCells[cellIndex].Color;

        if (type == ColorType.Primary)
        {
            PrimaryColor = selectedColor;
            DrawingBoard.PrimaryColor = PrimaryColor;
            return;
        }

        SecondaryColor = selectedColor;
        DrawingBoard.SecondaryColor = SecondaryColor;
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        var pickerClickPos = e.RelativeCursorPosition - InnerPadding;
        if (pickerClickPos.X < _pickerSize.X && pickerClickPos.Y < _pickerSize.Y &&
            pickerClickPos is { X: >= 0, Y: >= 0 })
        {
            SelectColor(pickerClickPos, ColorType.Primary);
        }

        base.OnMouseLeftDown(e);
    }

    protected override void OnMouseRightDown(MouseEventArgs e)
    {
        var pickerClickPos = e.RelativeCursorPosition - InnerPadding;
        if (pickerClickPos.X < _pickerSize.X && pickerClickPos.Y < _pickerSize.Y &&
            pickerClickPos is { X: >= 0, Y: >= 0 })
        {
            SelectColor(pickerClickPos, ColorType.Secondary);
        }

        base.OnMouseRightDown(e);
    }

    public override void Resize()
    {
        CalculateColorCellsPosition();

        _pickerSize = new Vector
        {
            X = (Colors.Count + ColorRows - 1) / ColorRows * ColorCellSize.X,
            Y = ColorRows * ColorCellSize.Y
        };

        MinSize = _pickerSize + InnerPadding * 2;

        ApplyResizing();
    }

    public override void Render()
    {
        base.Render();

        var startPos = GlobalPosition + InnerPadding;

        foreach (var cell in _colorCells)
        {
            Display.DrawRect(startPos + cell.Position, cell.Size, cell.Color);
        }
    }
}

public enum ColorType
{
    Primary,
    Secondary
}