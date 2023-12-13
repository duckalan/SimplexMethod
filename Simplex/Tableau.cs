using System.Text;

namespace Simplex;

public class Tableau
{
    private readonly double[,] tableau_;

    public Tableau(TargetFunction f, Constraint[] canonicalConstraints)
    {
        int n = canonicalConstraints[0].LeftPart.Length;
        int m = canonicalConstraints.Length;

        OrigVarsCount = f.Coefs.Length;
        SlackVarsCount = canonicalConstraints[0].LeftPart.Length - OrigVarsCount;

        // m + 1, так как необходимо добавить строку функции.
        // n + 2, так как необходимо добавить столбцы 
        // с функцией и свободными коэффициентами
        Height = m + 1;
        Width = n + 2;

        tableau_ = new double[Height, Width];

        // Запись левых коэффициентов ограничений
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                tableau_[i, j] = canonicalConstraints[i].LeftPart[j];
            }
        }

        // Запись столбца свободных членов
        for (int i = 0; i < m; i++)
        {
            tableau_[i, Width - 1] = canonicalConstraints[i].RightPart;
        }

        // Запись строки функции
        for (int j = 0; j < f.Coefs.Length; j++)
        {
            tableau_[Height - 1, j] = -f.Coefs[j];
        }

        for (int j = f.Coefs.Length; j < Width; j++)
        {
            if (j == Width - 2)
            {
                tableau_[Height - 1, j] = 1;
            }
            else if (j == Width - 1)
            {
                tableau_[Height - 1, j] = -f.Constant;
            }
        }
    }


    public int Height { get; }
    public int Width { get; }

    public int OrigVarsCount { get; }
    public int SlackVarsCount { get; }
    public int VarsCount => OrigVarsCount + SlackVarsCount;

    /// <summary>
    /// Столбец свободных членов
    /// </summary>
    public double[] B
    {
        get
        {
            var result = new double[Height];
            for (int i = 0; i < Height; i++)
            {
                result[i] = tableau_[i, Width - 1];
            }

            return result;
        }
    }

    public double[] FuncRow
    {
        get
        {
            var result = new double[Width];
            for (int j = 0; j < Width; j++)
            {
                result[j] = tableau_[Height - 1, j];
            }

            return result;
        }
    }

    public double this[int i, int j]
    {
        get => tableau_[i, j];
        set => tableau_[i, j] = value;
    }

    public override string ToString()
    {
        const int elementWidth = 10;

        var sb = new StringBuilder();

        for (int j = 0; j < OrigVarsCount; j++)
        {
            sb.Append($"{"x" + (j + 1),elementWidth}");
        }

        for (int j = 0; j < SlackVarsCount; j++)
        {
            sb.Append($"{"s" + (j + 1),elementWidth}");
        }

        sb.Append($"{"z",elementWidth}");
        sb.Append($"{"bi",elementWidth}");

        sb.Append('\n');

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                sb.Append($"{tableau_[i, j],elementWidth:F2}");
            }
            sb.Append('\n');
        }

        return sb.ToString();
    }
}
