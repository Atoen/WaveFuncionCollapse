namespace WaveFunctionCollapse.OverlappingModel;

public class InputArray
{
    public InputArray(int width, int height, int[] data)
    {
        Width = width;
        Height = height;
        Data = data;
    }

    public int Width { get; }
    public int Height { get; }
    public int[] Data { get; }
}

public class OverlappingModel : Model
{
    private readonly List<byte[]> _patterns;
    private readonly List<int> _colors;

    public OverlappingModel(InputArray inputArray, int patternSize, int width, int height, bool periodicInput, bool periodic,
        int symmetry, bool ground, Heuristic heuristic)
        : base(width, height, patternSize, periodic, heuristic)
    {
        var sample = new byte[inputArray.Data.Length];

        _colors = new List<int>();

        for (var i = 0; i < sample.Length; i++)
        {
            var pixelColor = inputArray.Data[i];
            var colorIndex = 0;

            while (colorIndex < _colors.Count && _colors[colorIndex] != pixelColor) colorIndex++;

            if (colorIndex == _colors.Count) _colors.Add(pixelColor);

            sample[i] = (byte) colorIndex;
        }

        _patterns = new List<byte[]>();

        var patternIndices = new Dictionary<long, int>();
        var weightList = new List<double>();

        var colorsCount = _colors.Count;
        var xMax = periodicInput ? inputArray.Height : inputArray.Width - patternSize + 1;
        var yMax = periodicInput ? inputArray.Height : inputArray.Width - patternSize + 1;

        for (var y = 0; y < yMax; y++)
        for (var x = 0; x < xMax; x++)
        {
            var patterns = GeneratePossiblePatterns(patternSize, sample, x, inputArray.Width, y, inputArray.Height);

            for (var symmetryIndex = 0; symmetryIndex < symmetry; symmetryIndex++)
            {
                var currentPattern = patterns[symmetryIndex];
                var hash = Hash(currentPattern, colorsCount);

                if (patternIndices.TryGetValue(hash, out var index))
                {
                    weightList[index]++;
                }

                else
                {
                    patternIndices.Add(hash, weightList.Count);
                    weightList.Add(1.0);

                    _patterns.Add(currentPattern);
                }
            }
        }

        Weights = weightList.ToArray();
        WeightsCount = Weights.Length;
        Ground = ground;

        Propagator = new int[4][][];

        for (var direction = 0; direction < 4; direction++)
        {
            Propagator[direction] = new int[WeightsCount][];

            for (var weightIndex = 0; weightIndex < WeightsCount; weightIndex++)
            {
                var list = new List<int>();

                for (var weightIndex2 = 0; weightIndex2 < WeightsCount; weightIndex2++)
                {
                    if (IsCompatible(_patterns[weightIndex], _patterns[weightIndex2], Dx[direction], Dy[direction],
                            patternSize))
                    {
                        list.Add(weightIndex2);
                    }
                }

                Propagator[direction][weightIndex] = new int[list.Count];

                for (var c = 0; c < list.Count; c++)
                {
                    Propagator[direction][weightIndex][c] = list[c];
                }
            }
        }
    }

    private static byte[][] GeneratePossiblePatterns(int patternSize, byte[] sample, int x1, int sampleWidth, int y1,
        int sampleHeight)
    {
        var patterns = new byte[8][];

        var pattern = GetPattern((dx, dy) => sample[(x1 + dx) % sampleWidth + (y1 + dy) % sampleHeight * sampleWidth],
            patternSize);

        patterns[0] = pattern;
        patterns[1] = Reflect(patterns[0], patternSize);
        patterns[2] = Rotate(patterns[0], patternSize);
        patterns[3] = Reflect(patterns[2], patternSize);
        patterns[4] = Rotate(patterns[2], patternSize);
        patterns[5] = Reflect(patterns[4], patternSize);
        patterns[6] = Rotate(patterns[4], patternSize);
        patterns[7] = Reflect(patterns[6], patternSize);

        return patterns;
    }

    private static bool IsCompatible(byte[] pattern1, byte[] pattern2, int dx, int dy, int patternSize)
    {
        var minX = dx < 0 ? 0 : dx;
        var maxX = dx < 0 ? dx + patternSize : patternSize;
        var minY = dy < 0 ? 0 : dy;
        var maxY = dy < 0 ? dy + patternSize : patternSize;

        for (var y = minY; y < maxY; y++)
        for (var x = minX; x < maxX; x++)
        {
            if (pattern1[x + patternSize * y] != pattern2[x - dx + patternSize * (y - dy)]) return false;
        }

        return true;
    }

    private static long Hash(byte[] pattern, int colorsCount)
    {
        var result = 0L;
        var power = 1L;

        for (var i = 0; i < pattern.Length; i++)
        {
            result += pattern[pattern.Length - 1 - i] * power;
            power *= colorsCount;
        }

        return result;
    }

    private static byte[] Reflect(byte[] pattern, int patternSize)
    {
        byte Transform(int x, int y) => pattern[patternSize - 1 - x + y * patternSize];

        return GetPattern(Transform, patternSize);
    }

    private static byte[] Rotate(byte[] pattern, int patternSize)
    {
        byte Transform(int x, int y) => pattern[patternSize - 1 - y + x * patternSize];

        return GetPattern(Transform, patternSize);
    }

    private static byte[] GetPattern(Func<int, int, byte> transform, int patternSize)
    {
        var result = new byte[patternSize * patternSize];

        for (var y = 0; y < patternSize; y++)
        for (var x = 0; x < patternSize; x++)
        {
            var index = x + y * patternSize;
            result[index] = transform(x, y);
        }

        return result;
    }

    public int[] Save()
    {
        var bitmap = new int[Width * Height];

        try
        {
            if (Observed[0] >= 0)
            {
                NewMethod(bitmap);
            }
            else
            {
                NewMethod1(bitmap);
            }

            Console.Title = "Model succeeded";
        }
        catch
        {
            Console.Title = "Model failed";
        }

        return bitmap;
    }

    private void NewMethod1(int[] bitmap)
    {
        for (var i = 0; i < Wave.Length; i++)
        {
            int contributors = 0, r = 0, g = 0, b = 0;
            int x = i % Width, y = i / Width;

            for (var dy = 0; dy < PatternSize; dy++)
            for (var dx = 0; dx < PatternSize; dx++)
            {
                var sx = x - dx;
                if (sx < 0) sx += Width;

                var sy = y - dy;
                if (sy < 0) sy += Height;

                var s = sx + sy * Width;
                if (!Periodic && (sx + PatternSize > Width || sy + PatternSize > Height || sx < 0 || sy < 0))
                {
                    continue;
                }

                for (var t = 0; t < WeightsCount; t++)
                {
                    if (!Wave[s][t]) continue;

                    contributors++;
                    var argb = _colors[_patterns[t][dx + dy * PatternSize]];
                    r += (argb & 0xff0000) >> 16;
                    g += (argb & 0xff00) >> 8;
                    b += argb & 0xff;
                }
            }

            bitmap[i] = unchecked((int) 0xff000000 | ((r / contributors) << 16) | ((g / contributors) << 8) |
                                  b / contributors);
        }
    }

    private void NewMethod(int[] bitmap)
    {
        for (var y = 0; y < Height; y++)
        {
            var dy = y < Height - PatternSize + 1 ? 0 : PatternSize - 1;
            for (var x = 0; x < Width; x++)
            {
                var dx = x < Width - PatternSize + 1 ? 0 : PatternSize - 1;
                bitmap[x + y * Width] =
                    _colors[_patterns[Observed[x - dx + (y - dy) * Width]][dx + dy * PatternSize]];
            }
        }
    }
}