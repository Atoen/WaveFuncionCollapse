using ConsoleGUI;
using ConsoleGUI.UI;
using ConsoleGUI.UI.Widgets;

namespace WaveFunctionCollapse;

public class DrawingModeSelector : ContentControl
{
    public DrawingModeSelector()
    {
        InnerPadding = Vector.Zero;

        var grid = new Grid
        {
            ShowGridLines = true,
            GridLinesColor = Color.Black
        };
        
        grid.Columns.Add(new Column());
        grid.Columns.Add(new Column());
        grid.Rows.Add(new Row());

        var variable = new Variable();

        var normal = new RadioButton(variable, 0)
        {
            Text = new Text("Pen"),
            OnClick = () => DrawingBoard!.DrawMode = DrawMode.Pen
            
        };

        var fill = new RadioButton(variable, 1)
        {
            Text = new Text("Fill"),
            OnClick = () => DrawingBoard!.DrawMode = DrawMode.Fill
        };
        
        grid.SetColumnAndRow(normal, 0, 0);
        grid.SetColumnAndRow(fill, 1, 0);

        Content = grid;
    }

    public required DrawingBoard DrawingBoard { get; set; }
}