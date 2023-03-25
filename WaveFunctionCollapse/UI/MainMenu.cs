using ConsoleGUI.UI;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Visuals.Figlet;
using WaveFunctionCollapse.OverlappingModel;

namespace WaveFunctionCollapse.UI;

public class MainMenu
{
    private MainMenu()
    {
        var grid = new Grid
        {
            Color = Color.Red,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        grid.Columns.Add(new Column());
        grid.Columns.Add(new Column());
        grid.Columns.Add(new Column());
        grid.Rows.Add(new Row());
        grid.Rows.Add(new Row());
        grid.Rows.Add(new Row());
        grid.Rows.Add(new Row());
        grid.Rows.Add(new Row());
        grid.Rows.Add(new Row());
        grid.Rows.Add(new Row());

        _drawingBoard = new DrawingBoard(20, 12);
        grid.SetColumnAndRow(_drawingBoard, 0, 0);
        grid.SetColumnSpanAndRowSpan(_drawingBoard, 1, 6);

        var generateButton = new Button
        {
            Text = new BigText("generate", Font.CalvinS)
        };
        grid.SetColumnAndRow(generateButton, 0, 6);

        _resultDisplay = new Canvas(30, 20);
        grid.SetColumnAndRow(_resultDisplay, 2, 0);
        grid.SetColumnSpanAndRowSpan(_resultDisplay, 1, 7);

        var groundToggleButton = new ToggleButton<bool>
        {
            ToggleManager = new ToggleStateManager<bool>("Ground", true, false)
        };
        groundToggleButton.Select(false);
        grid.SetColumnAndRow(groundToggleButton, 1, 0);

        var symmetryToggleButton = new ToggleButton<int>
        {
            ToggleManager = new ToggleStateManager<int>("Symmetry", 2, 4, 8)
        };
        grid.SetColumnAndRow(symmetryToggleButton, 1, 1);

        var periodicInputToggleButton = new ToggleButton<bool>
        {
            ToggleManager = new ToggleStateManager<bool>("Periodic Input", true, false)
        };
        grid.SetColumnAndRow(periodicInputToggleButton, 1, 2);

        var periodicOutputToggleButton = new ToggleButton<bool>
        {
            ToggleManager = new ToggleStateManager<bool>("Periodic Output", true, false)
        };
        grid.SetColumnAndRow(periodicOutputToggleButton, 1, 3);

        generateButton.OnClick = SetUpModel;

        void SetUpModel()
        {
            var ground = groundToggleButton.ToggleState;
            var symmetry = symmetryToggleButton.ToggleState;
            var periodicInput = periodicInputToggleButton.ToggleState;
            var periodicOutput = periodicOutputToggleButton.ToggleState;

            OverlappingModel(_drawingBoard.Canvas.Buffer, ground, symmetry, periodicInput, periodicOutput);
        }
    }

    private static MainMenu? _instance;
    private readonly DrawingBoard _drawingBoard;
    private readonly Canvas _resultDisplay;

    public static void Show() => _instance ??= new MainMenu();

    private void OverlappingModel(PixelBuffer inputBuffer, bool ground, int symmetry, bool periodicInput, bool periodic)
    {
        var dataArray = new int[inputBuffer.Size.X * inputBuffer.Size.Y];

        var i = 0;
        foreach (var pixel in inputBuffer)
        {
            var color = pixel.Bg;
            dataArray[i] = color.ToArgb();
            i++;
        }

        var input = new InputArray(inputBuffer.Size.X, inputBuffer.Size.Y, dataArray);

        var model = new OverlappingModel.OverlappingModel(input, 5, _resultDisplay.Buffer.Size.X,
            _resultDisplay.Buffer.Size.Y, periodicInput: periodicInput, periodic: periodic,
            symmetry: symmetry, ground: ground, heuristic: Model.Heuristic.Entropy);

        var seed = Random.Shared.Next();

        var success = model.Run(seed, -1);

        if (!success)
        {
            Console.Title = "Model failed.";
            return;
        }

        var output = model.Save();
        DisplayOutput(output);
    }

    private void DisplayOutput(int[] array)
    {
        for (var x = 0; x < _resultDisplay.Buffer.Size.X; x++)
        for (var y = 0; y < _resultDisplay.Buffer.Size.Y; y++)
        {
            _resultDisplay.Buffer[x, y].Bg = Color.FromArgb(array[y * _resultDisplay.Buffer.Size.X + x]);
        }
    }
}