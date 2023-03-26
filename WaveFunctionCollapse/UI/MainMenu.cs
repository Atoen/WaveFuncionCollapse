using ConsoleGUI;
using ConsoleGUI.UI;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Visuals;
using ConsoleGUI.Visuals.Figlet;
using WaveFunctionCollapse.OverlappingModel;

namespace WaveFunctionCollapse.UI;

public class MainMenu
{
    private MainMenu()
    {
        var mainGrid = new Grid
        {
            Color = Color.SlateGray,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        mainGrid.Columns.Add(new Column());
        mainGrid.Columns.Add(new Column());
        mainGrid.Columns.Add(new Column());
        mainGrid.Rows.Add(new Row());
        mainGrid.Rows.Add(new Row());

        _drawingBoard = new DrawingBoard(20, 12)
        {
            Color = Color.Empty,
            InnerPadding = Vector.Zero
        };
        
        mainGrid.SetColumnAndRow(_drawingBoard, 0, 0);
        mainGrid.SetColumnSpanAndRowSpan(_drawingBoard, 1, 1);

        var generateButton = new Button
        {
            Text = new BigText("generate", Font.CalvinS)
        };
        mainGrid.SetColumnAndRow(generateButton, 0, 1);

        _resultDisplay = new Canvas(30, 20);
        mainGrid.SetColumnAndRow(_resultDisplay, 2, 0);
        mainGrid.SetColumnSpanAndRowSpan(_resultDisplay, 1, 2);

        var modelOptionsGrid = new Grid
        {
            Color = Color.Empty,
            InnerPadding = Vector.Zero
        };
        
        modelOptionsGrid.Columns.Add(new Column());
        modelOptionsGrid.Rows.Add(new Row());
        modelOptionsGrid.Rows.Add(new Row());
        modelOptionsGrid.Rows.Add(new Row());
        modelOptionsGrid.Rows.Add(new Row());

        var groundToggleButton = new ToggleButton<bool>
        {
            ToggleManager = new ToggleStateManager<bool>("Ground", true, false)
        };
        groundToggleButton.Select(false);
        modelOptionsGrid.SetColumnAndRow(groundToggleButton, 0, 0);

        var symmetryToggleButton = new ToggleButton<int>
        {
            ToggleManager = new ToggleStateManager<int>("Symmetry", 2, 4, 8)
        };
        modelOptionsGrid.SetColumnAndRow(symmetryToggleButton, 0, 1);

        var periodicInputToggleButton = new ToggleButton<bool>
        {
            ToggleManager = new ToggleStateManager<bool>("Periodic Input", true, false)
        };
        modelOptionsGrid.SetColumnAndRow(periodicInputToggleButton, 0, 2);

        var periodicOutputToggleButton = new ToggleButton<bool>
        {
            ToggleManager = new ToggleStateManager<bool>("Periodic Output", true, false)
        };
        modelOptionsGrid.SetColumnAndRow(periodicOutputToggleButton, 0, 3);
        
        mainGrid.SetColumnAndRow(modelOptionsGrid, 1, 0);

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