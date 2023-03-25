using ConsoleGUI;
using ConsoleGUI.UI;

namespace WaveFunctionCollapse.EvenSimplerModel;

public class Parser
{
    public Parser(PixelBuffer inputBuffer)
    {
        _inputBuffer = inputBuffer;
    }

    private readonly PixelBuffer _inputBuffer;
    public readonly Dictionary<Color, int> Weights = new();
    public readonly HashSet<Rule> Rules = new();

    public record struct Rule(Color Current, Color Neighbour, Vector AllowedDirection);

    public void ParseInput()
    {
        var pos = Vector.Zero;

        for (var x = 0; x < _inputBuffer.Size.X; x++)
        for (var y = 0; y < _inputBuffer.Size.Y; y++)
        {
            pos.X = x;
            pos.Y = y;

            var color = _inputBuffer[pos].Bg;

            if (!Weights.ContainsKey(color)) Weights.Add(color, 1);
            else Weights[color]++;

            AddRules(color, pos);
        }
    }

    private void AddRules(Color currentColor, Vector position)
    {
        var directions = GetValidRuleDirection(position);

        foreach (var direction in directions)
        {
            var otherColor = _inputBuffer[position + direction].Bg;

            Rules.Add(new Rule(currentColor, otherColor, direction));
        }
    }

    private List<Vector> GetValidRuleDirection(Vector position)
    {
        var directions = new List<Vector>();

        if (position.X > 0) directions.Add(Vector.Left);
        if (position.X < _inputBuffer.Size.X - 1) directions.Add(Vector.Right);
        if (position.Y > 0) directions.Add(Vector.Up);
        if (position.Y < _inputBuffer.Size.Y - 1) directions.Add(Vector.Down);

        return directions;
    }
}