using ConsoleGUI;

namespace WaveFunctionCollapse.EvenSimplerModel;

public class Model
{
    public Model(Vector outputSize, Dictionary<Color, int> weights, HashSet<Parser.Rule> rules)
    {
        _outputSize = outputSize;
        _rules = rules;

        _waveFunction = WaveFunction.Init(outputSize, weights);
    }

    private readonly Vector _outputSize;
    private readonly HashSet<Parser.Rule> _rules;

    private readonly WaveFunction _waveFunction;

    public Color[,] Run()
    {
        while (!_waveFunction.IsCollapsed())
        {
            Iterate();
        }

        return _waveFunction.GetCollapsedResult();
    }

    private void Iterate()
    {
        var minEntropyPoint = MinimumEntropyPosition();

        _waveFunction.Collapse(minEntropyPoint);

        Propagate(minEntropyPoint);
    }

    private Vector MinimumEntropyPosition()
    {
        var lowestFoundEntropy = double.MaxValue;
        var minEntropyPosition = new Vector();

        var pos = Vector.Zero;

        for (var x = 0; x < _outputSize.X; x++)
        for (var y = 0; y < _outputSize.Y; y++)
        {
            pos.X = x;
            pos.Y = y;

            if (_waveFunction.PossiblePixelStates(pos).Count == 1) continue;

            var entropy = _waveFunction.Entropy(pos);
            var noise = Random.Shared.NextDouble() / 1000;
            var entropyWithNoise = entropy + noise;

            if (!(lowestFoundEntropy > entropyWithNoise)) continue;

            lowestFoundEntropy = entropyWithNoise;
            minEntropyPosition = pos;
        }

        return minEntropyPosition;
    }

    private void Propagate(Vector position)
    {
        var stack = new Stack<Vector>();
        stack.Push(position);

        while (stack.Count > 0)
        {
            var currentPosition = stack.Pop();
            var currentPossibleStates = _waveFunction.PossiblePixelStates(position);

            foreach (var direction in GetValidRuleDirection(currentPosition))
            {
                var otherPosition = currentPosition + direction;

                foreach (var otherColor in _waveFunction.PossiblePixelStates(otherPosition))
                {
                    var possible = false;

                    foreach (var possibleState in currentPossibleStates)
                    {
                        var rule = new Parser.Rule(possibleState, otherColor, direction);
                        if (!_rules.Contains(rule)) continue;

                        possible = true;
                        break;
                    }

                    if (possible) continue;

                    _waveFunction.Constrain(otherPosition, otherColor);
                    stack.Push(otherPosition);
                }
            }
        }
    }

    private List<Vector> GetValidRuleDirection(Vector position)
    {
        var directions = new List<Vector>();

        if (position.X > 0) directions.Add(Vector.Left);
        if (position.X < _outputSize.X - 1) directions.Add(Vector.Right);
        if (position.Y > 0) directions.Add(Vector.Up);
        if (position.Y < _outputSize.Y - 1) directions.Add(Vector.Down);

        return directions;
    }
}