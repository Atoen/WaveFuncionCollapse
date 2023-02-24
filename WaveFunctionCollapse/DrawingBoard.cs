using ConsoleGUI;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Visuals;

namespace WaveFunctionCollapse;

public class DrawingBoard : Grid
{
    public DrawingBoard()
    {
        Color = Color.SlateGray;
        VerticalAlignment = VerticalAlignment.Bottom;

        Columns.Add(new Column());
        Columns.Add(new Column());
        Columns.Add(new Column());
        Rows.Add(new Row());
        Rows.Add(new Row());

        Canvas = new Canvas(new Vector(30, 20))
        {
            BorderStyle = BorderStyle.Rounded,
            BorderColor = Color.Black,
            ShowBorder = true,

            DrawingBoard = this
        };
        SetColumnAndRow(Canvas, 0, 1);
        SetColumnSpanAndRowSpan(Canvas, 3, 1);

        var colors = new List<Color>
        {
            Color.Black, Color.Gray, Color.Red, Color.Orange, Color.Yellow, Color.Green,
            Color.LightSkyBlue, Color.Blue, Color.Purple,
            Color.White, Color.LightGray, Color.Brown, Color.Pink, Color.Lime
        };

        ColorPicker = new ColorPicker
        {
            Colors = colors,
            ColorCellSize = (3, 2),
            ColorRows = 2,

            BorderStyle = BorderStyle.Rounded,
            BorderColor = Color.Black,
            ShowBorder = true,

            DrawingBoard = this
        };
        SetColumnAndRow(ColorPicker, 1, 0);

        SelectedColorsDisplay = new SelectedColorsDisplay
        {
            DrawingBoard = this,
            ColorCellSize = (4, 2)
        };
        SetColumnAndRow(SelectedColorsDisplay, 0, 0);

        DrawingModeSelector = new DrawingModeSelector
        {
            DrawingBoard = this
        };
        SetColumnAndRow(DrawingModeSelector, 2, 0);
    }

    public ColorPicker ColorPicker { get; }
    public Canvas Canvas { get; }
    public SelectedColorsDisplay SelectedColorsDisplay { get; }
    public DrawingModeSelector DrawingModeSelector { get; }

    public Color PrimaryColor { get; set; } = Color.Black;
    public Color SecondaryColor { get; set; } = Color.White;
    public DrawMode DrawMode { get; set; } = DrawMode.Pen;
}

public enum DrawMode
{
    Pen,
    Fill
}