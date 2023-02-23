using ConsoleGUI;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI;

namespace WaveFunctionCollapse;

public class SelectedColorsDisplay : Control
{
    public SelectedColorsDisplay() => ShowFocusedBorder = false;

    public Vector ColorCellSize { get; set; } = new(5, 3);
    public int Gap { get; set; } = 1;
    public required DrawingBoard DrawingBoard { get; set; }

    public override void Resize()
    {
        MinSize = ColorCellSize with { X = ColorCellSize.X * 2 + Gap } + InnerPadding * 2;

        ApplyResizing();
    }

    public override void Render()
    {
        base.Render();

        var cellPos = GlobalPosition + InnerPadding;

        Display.DrawRect(cellPos, ColorCellSize, DrawingBoard.PrimaryColor);

        cellPos.X += ColorCellSize.X + Gap;

        Display.DrawRect(cellPos, ColorCellSize, DrawingBoard.SecondaryColor);
    }
}