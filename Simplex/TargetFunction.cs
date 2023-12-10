using System.Text;

namespace Simplex;

public class TargetFunction
{
    private readonly double[] _coefs;
    private readonly double _constant;

    /// <param name="coefs">Коэффициенты для x1, x2, x3 и т.д.</param>
    /// <param name="constant">Константа функции</param>
    public TargetFunction(double[] coefs, double constant)
    {
        _coefs = coefs;
        _constant = constant;
    }

    public ReadOnlySpan<double> Coefs => _coefs.AsSpan();

    public double Constant => _constant;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("z(x) = ");

        for (int i = 0; i < _coefs.Length; i++)
        {
            if (i != _coefs.Length - 1)
            {
                sb.Append($"{"x" + (i + 1)}*({_coefs[i]:F2}) + ");
            }
            else
            {
                sb.Append($"{"x" + (i + 1)}*({_coefs[i]:F2})");
            }

        }

        if (_constant != 0)
        {
            sb.Append($" + ({_constant})");
        }

        return sb.ToString();
    }
}