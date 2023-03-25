namespace WaveFunctionCollapse.OverlappingModel;

public abstract class Model
{
    protected bool[][] Wave = null!;

    protected int[][][] Propagator = null!;
    private int[][][] _compatible = null!;
    protected int[] Observed = null!;

    private (int, int)[] _stack = null!;
    private int _stackSize, _observedSoFar;

    protected readonly int Width;
    protected readonly int Height;
    protected readonly int PatternSize;

    protected int WeightsCount;
    protected readonly bool Periodic;
    protected bool Ground;

    protected double[] Weights = null!;
    private double[] _weightLogWeights = null!, _distribution = null!;

    private int[] _sumsOfOnes = null!;
    private double _sumOfWeights, _sumOfWeightLogWeights, _startingEntropy;
    private double[] _sumsOfWeights = null!;
    private double[] _sumsOfWeightLogWeights = null!;
    private double[] _entropies = null!;


    protected static readonly int[] Dx = { -1, 0, 1, 0 };
    protected static readonly int[] Dy = { 0, 1, 0, -1 };
    private static readonly int[] Opposite = { 2, 3, 0, 1 };

    public enum Heuristic { Entropy, Mrv, Scanline }

    private readonly Heuristic _heuristic;

    protected Model(int width, int height, int patternSize, bool periodic, Heuristic heuristic)
    {
        Width = width;
        Height = height;
        PatternSize = patternSize;
        Periodic = periodic;
        _heuristic = heuristic;
    }

    private void Init()
    {
        Wave = new bool[Width * Height][];

        _compatible = new int[Wave.Length][][];

        for (var i = 0; i < Wave.Length; i++)
        {
            Wave[i] = new bool[WeightsCount];
            _compatible[i] = new int[WeightsCount][];

            for (var t = 0; t < WeightsCount; t++)
            {
                _compatible[i][t] = new int[4];
            }
        }

        _distribution = new double[WeightsCount];
        Observed = new int[Width * Height];

        CreateEntropyTable();

        _startingEntropy = Math.Log(_sumOfWeights) - _sumOfWeightLogWeights / _sumOfWeights;

        _sumsOfOnes = new int[Width * Height];
        _sumsOfWeights = new double[Width * Height];
        _sumsOfWeightLogWeights = new double[Width * Height];
        _entropies = new double[Width * Height];

        _stack = new (int, int)[Wave.Length * WeightsCount];
        _stackSize = 0;
    }

    private void CreateEntropyTable()
    {
        _weightLogWeights = new double[WeightsCount];
        _sumOfWeights = 0;
        _sumOfWeightLogWeights = 0;

        for (var i = 0; i < WeightsCount; i++)
        {
            _weightLogWeights[i] = Weights[i] * Math.Log(Weights[i]);
            _sumOfWeights += Weights[i];
            _sumOfWeightLogWeights += _weightLogWeights[i];
        }
    }

    public bool Run(int seed, int limit)
    {
        if (Wave == null) Init();

        Clear();
        var random = new Random(seed);

        for (var iteration = 0; iteration < limit || limit < 0; iteration++)
        {
            var node = NextUnobservedNode(random.NextDouble());
            if (node >= 0)
            {
                Observe(node, random.NextDouble());

                var success = Propagate();

                if (!success) return false;
            }
            else
            {
                for (var i = 0; i < Wave!.Length; i++)
                for (var tile = 0; tile < WeightsCount; tile++)
                {
                    if (!Wave[i][tile]) continue;

                    Observed[i] = tile;
                    break;
                }

                return true;
            }
        }

        return true;
    }

    private int NextUnobservedNode(double randomDouble)
    {
        if (_heuristic == Heuristic.Scanline)
        {
            for (var i = _observedSoFar; i < Wave.Length; i++)
            {
                if (!Periodic && (i % Width + PatternSize > Width || i / Width + PatternSize > Height)) continue;
                if (_sumsOfOnes[i] > 1)
                {
                    _observedSoFar = i + 1;
                    return i;
                }
            }

            return -1;
        }

        var min = 1E+4;
        var minEntropyNode = -1;
        for (var i = 0; i < Wave.Length; i++)
        {
            if (!Periodic && (i % Width + PatternSize > Width || i / Width + PatternSize > Height)) continue;
            var remainingValues = _sumsOfOnes[i];
            var entropy = _heuristic == Heuristic.Entropy ? _entropies[i] : remainingValues;

            if (remainingValues <= 1 || !(entropy <= min)) continue;

            var noise = 1E-6 * randomDouble;

            if (entropy + noise >= min) continue;

            min = entropy + noise;
            minEntropyNode = i;
        }

        return minEntropyNode;
    }

    private void Observe(int node, double randomDouble)
    {
        var waveCell = Wave[node];
        for (var t = 0; t < WeightsCount; t++)
        {
            _distribution[t] = waveCell[t] ? Weights[t] : 0.0;
        }

        var r = _distribution.RandomWeights(randomDouble);
        for (var t = 0; t < WeightsCount; t++)
        {
            if (waveCell[t] != (t == r)) Ban(node, t);
        }
    }

    private bool Propagate()
    {
        while (_stackSize > 0)
        {
            var (i1, t1) = _stack[_stackSize - 1];
            _stackSize--;

            var x1 = i1 % Width;
            var y1 = i1 / Width;

            for (var direction = 0; direction < 4; direction++)
            {
                var x2 = x1 + Dx[direction];
                var y2 = y1 + Dy[direction];

                if (!Periodic && (x2 < 0 || y2 < 0 || x2 + PatternSize > Width || y2 + PatternSize > Height)) continue;

                if (x2 < 0) x2 += Width;
                else if (x2 >= Width) x2 -= Width;
                if (y2 < 0) y2 += Height;
                else if (y2 >= Height) y2 -= Height;

                var i2 = x2 + y2 * Width;
                var p = Propagator[direction][t1];
                var compat = _compatible[i2];

                foreach (var t2 in p)
                {
                    var comp = compat[t2];

                    comp[direction]--;
                    if (comp[direction] == 0) Ban(i2, t2);
                }
            }
        }

        return _sumsOfOnes[0] > 0;
    }

    private void Ban(int i, int tile)
    {
        Wave[i][tile] = false;

        var tileCompatibilities = _compatible[i][tile];
        for (var direction = 0; direction < 4; direction++)
        {
            tileCompatibilities[direction] = 0;
        }

        _stack[_stackSize] = (i, tile);
        _stackSize++;

        _sumsOfOnes[i] -= 1;
        _sumsOfWeights[i] -= Weights[tile];
        _sumsOfWeightLogWeights[i] -= _weightLogWeights[tile];

        var sum = _sumsOfWeights[i];
        _entropies[i] = Math.Log(sum) - _sumsOfWeightLogWeights[i] / sum;
    }

    private void Clear()
    {
        for (var i = 0; i < Wave.Length; i++)
        {
            for (var t = 0; t < WeightsCount; t++)
            {
                Wave[i][t] = true;
                for (var d = 0; d < 4; d++) _compatible[i][t][d] = Propagator[Opposite[d]][t].Length;
            }

            _sumsOfOnes[i] = Weights.Length;
            _sumsOfWeights[i] = _sumOfWeights;
            _sumsOfWeightLogWeights[i] = _sumOfWeightLogWeights;
            _entropies[i] = _startingEntropy;
            Observed[i] = -1;
        }
        _observedSoFar = 0;

        if (!Ground) return;

        for (var x = 0; x < Width; x++)
        {
            for (var t = 0; t < WeightsCount - 1; t++)
            {
                Ban(x + (Height - 1) * Width, t);
            }

            for (var y = 0; y < Height - 1; y++)
            {
                Ban(x + y * Width, WeightsCount - 1);
            }
        }

        Propagate();
    }
}

public static class RandomExtensions
{
    public static int RandomWeights(this double[] weights, double r)
    {
        var sum = weights.Sum();
        var threshold = r * sum;

        double partialSum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            partialSum += weights[i];
            if (partialSum >= threshold) return i;
        }

        return 0;
    }
}