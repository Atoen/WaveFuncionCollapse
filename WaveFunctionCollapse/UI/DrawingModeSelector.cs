using ConsoleGUI.UI;
using ConsoleGUI.UI.Widgets;

namespace WaveFunctionCollapse.UI;

public class DrawingModeSelector : Grid
{
    public DrawingModeSelector()
    {
        ShowGridLines = true;
        GridLinesColor = Color.Black;

        Columns.Add(new Column());
        Columns.Add(new Column());
        Rows.Add(new Row());

        var variable = new Variable();

        var pen = new RadioButton(variable, 0)
        {
            Text = new Text("Pen"),
            OnClick = () => DrawingBoard!.DrawMode = DrawMode.Pen
        };

        var fill = new RadioButton(variable, 1)
        {
            Text = new Text("Fill"),
            OnClick = () => DrawingBoard!.DrawMode = DrawMode.Fill
        };

        SetColumnAndRow(pen, 0, 0);
        SetColumnAndRow(fill, 1, 0);
    }

    public required DrawingBoard DrawingBoard { get; set; }
}