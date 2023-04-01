using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Visuals;

namespace WaveFunctionCollapse.UI;

public class DrawingBoard : Grid
{
    public DrawingBoard(int width, int height)
    {
        Color = Color.SlateGray;
        VerticalAlignment = VerticalAlignment.Top;
        HorizontalAlignment = HorizontalAlignment.Left;

        Columns.Add(new Column());
        Columns.Add(new Column());
        Rows.Add(new Row());
        Rows.Add(new Row());

        Canvas = new InputCanvas(width, height)
        {
            BorderStyle = BorderStyle.Rounded,
            BorderColor = Color.Black,
            ShowBorder = true,
            CanGripResize = true,

            DrawingBoard = this
        };
        SetColumnAndRow(Canvas, 1, 1);

        var colors = new List<Color>
        {
            Color.Red, Color.Yellow, Color.Orange,
            Color.Blue, Color.Green, Color.Purple,
            Color.Brown, Color.Olive, Color.Tan,
            Color.Navy, Color.SeaGreen, Color.SkyBlue,
            Color.LightPink, Color.Lavender, Color.Coral,
            Color.White, Color.Gray, Color.Black
        };

        ColorPicker = new ColorPicker
        {
            Colors = colors,
            ColorCellSize = (3, 2),
            ColorRows = 6,

            BorderStyle = BorderStyle.Rounded,
            BorderColor = Color.Black,
            ShowBorder = true,

            DrawingBoard = this
        };
        SetColumnAndRow(ColorPicker, 0, 1);

        SelectedColorsDisplay = new SelectedColorsDisplay
        {
            DrawingBoard = this,
            ColorCellSize = (4, 3)
        };
        SetColumnAndRow(SelectedColorsDisplay, 0, 0);

        DrawingModeSelector = new DrawingModeSelector
        {
            DrawingBoard = this
        };
        SetColumnAndRow(DrawingModeSelector, 1, 0);
    }

    public ColorPicker ColorPicker { get; }
    public InputCanvas Canvas { get; }
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