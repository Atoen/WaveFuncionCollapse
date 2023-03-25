using ConsoleGUI;

namespace WaveFunctionCollapse.EvenSimplerModel;

public class WaveFunction
{
    private WaveFunction(HashSet<Color>[,] coefficientsMatrix, Dictionary<Color, int> weights)
    {
        _coefficientsMatrix = coefficientsMatrix;
        _weights = weights;
    }

    private readonly HashSet<Color>[,] _coefficientsMatrix;
    private readonly Dictionary<Color, int> _weights;

    public static WaveFunction Init(Vector outputSize, Dictionary<Color, int> weights)
    {
        var coefficientsMatrix = InitializeCoefficientMatrix(outputSize, weights.Keys.ToHashSet());
        return new WaveFunction(coefficientsMatrix, weights);
    }

    private static HashSet<Color>[,] InitializeCoefficientMatrix(Vector size, HashSet<Color> allowedColors)
    {
        var matrix = new HashSet<Color>[size.X, size.Y];

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        {
            matrix[x, y] = new HashSet<Color>(allowedColors);
        }

        return matrix;
    }

    public bool IsCollapsed()
    {
        return _coefficientsMatrix.Cast<HashSet<Color>>().All(colorOptions => colorOptions.Count <= 1);
    }

    public HashSet<Color> PossiblePixelStates(Vector position)
    {
        return _coefficientsMatrix[position.X, position.Y];
    }

    public double Entropy(Vector position)
    {
        var weightsSum = 0;
        var weightLogSum = 0.0;

        foreach (var possibleState in _coefficientsMatrix[position.X, position.Y])
        {
            var weight = _weights[possibleState];
            weightsSum += weight;

            weightLogSum += weight * Math.Log(weight);
        }

        var entropy = Math.Log(weightsSum) - weightLogSum / weightsSum;

        return entropy;
    }

    public void Collapse(Vector position)
    {
        var possibleStates = _coefficientsMatrix[position.X, position.Y];
        var keyValuePairs = _weights.Where(pair => possibleStates.Contains(pair.Key)).ToList();
        var totalWeights = keyValuePairs.Sum(pair => pair.Value);

        var random = Random.Shared.Next(0, totalWeights);

        var selectedColor = Color.Purple;

        foreach (var (color, weight) in keyValuePairs)
        {
            if (random < weight)
            {
                selectedColor = color;
                break;
            }

            random -= weight;
        }

        // var random = Random.Shared.NextDouble() * totalWeights;
        // var selectedColor = keyValuePairs[0].Key;
        //
        // foreach (var (color, weight) in keyValuePairs)
        // {
        //     random -= weight;
        //     if (random > 0) continue;
        //     
        //     selectedColor = color;
        //     break;
        // }

        _coefficientsMatrix[position.X, position.Y].Clear();
        _coefficientsMatrix[position.X, position.Y].Add(selectedColor);
    }

    public void Constrain(Vector position, Color incompatibleColor)
    {
        _coefficientsMatrix[position.X, position.Y].Remove(incompatibleColor);
    }

    public Color[,] GetCollapsedResult()
    {
        var width = _coefficientsMatrix.GetLength(0);
        var height = _coefficientsMatrix.GetLength(1);

        var collapsed = new Color[width, height];

        var pos = new Vector();

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            pos.X = x;
            pos.Y = y;

            var result = Color.Purple;

            try
            {
                result = _coefficientsMatrix[pos.X, pos.Y].First();
            }
            catch (Exception)
            {
            }


            collapsed[x, y] = result;
        }

        return collapsed;
    }
}
