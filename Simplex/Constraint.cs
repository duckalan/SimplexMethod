using System.Data;
using System.Text;

namespace Simplex;

public enum ConstraintType
{
    Equal,
    LowerOrEqual,
    GreaterOrEqual
}

public class Constraint
{
    /// <summary>
    /// Все коэффициенты левой и правой части со своими знаками
    /// </summary>
    private readonly double[] _constraintCoefs;

    public Constraint(double[] constraintCoefs, ConstraintType type)
    {
        _constraintCoefs = new double[constraintCoefs.Length];
        constraintCoefs.CopyTo(_constraintCoefs, 0);

        Type = type;
    }

    public Constraint(double[] leftPart, double rightPart, ConstraintType type)
    {
        _constraintCoefs = new double[leftPart.Length + 1];
        leftPart.CopyTo(_constraintCoefs, 0);
        _constraintCoefs[_constraintCoefs.Length - 1] = rightPart;
        Type = type;
    }


    public ConstraintType Type { get; init; }

    /// <summary>
    /// Количество коэффициентов в левой части вместе с 
    /// коэффициентом из правой части
    /// </summary>
    public int FullLength => _constraintCoefs.Length;

    /// <summary>
    /// Все коэффициенты ограничения, включая левую и правую части
    /// со своими исходными коэффициентами
    /// </summary>
    public double[] FullConstraint => _constraintCoefs;

    public ReadOnlySpan<double> LeftPart => _constraintCoefs.AsSpan(0, FullLength - 1);

    public double RightPart => _constraintCoefs[FullLength - 1];

    public override string ToString()
    {
        string relationalSign;

        switch (Type)
        {
            case ConstraintType.Equal:
                relationalSign = "=";
                break;
            case ConstraintType.LowerOrEqual:
                relationalSign = "<=";
                break;
            case ConstraintType.GreaterOrEqual:
                relationalSign = ">=";
                break;
            default:
                relationalSign = "";
                break;
        }

        var sb = new StringBuilder();
        const int elementWidth = 10;

        for (int i = 0; i < FullLength - 1; i++)
        {
            if (i < FullLength - 2)
            {
                var elementString = $"x{i + 1}*({_constraintCoefs[i]:F2}) + ";
                sb.Append($"{elementString, -elementWidth}");
            }
            else
            {
                var elementString = $"x{i + 1}*({_constraintCoefs[i]:F2})";
                sb.Append($"{elementString, -elementWidth}");
            }
        }

        sb.Append($" {relationalSign} ");
        sb.Append($"{RightPart,-5:F2}");

        return sb.ToString();
    }
}

public static class ConstraintArrayExtensions
{
    public static Constraint[] ToCanonicalForm(this Constraint[] constraints)
    {
        // Количество ограничений
        int m = constraints.Length;

        // m ограничений-равенств преобразуются в m+1 неравенств
        foreach (var constraint in constraints)
        {
            if (constraint.Type == ConstraintType.Equal)
            {
                m++;
                break;
            }
        }

        int origN = constraints[0].LeftPart.Length;
        // Количество переменных в ограничениях, включая slack-переменные
        int n = origN + m;

        var result = new Constraint[m];
        // Основное преобразование
        for (int i = 0; i < constraints.Length; i++)
        {
            double[] leftPart = new double[n];

            // Заполнение исходной левой части
            for (int j = 0; j < origN; j++)
            {
                leftPart[j] = constraints[i].LeftPart[j];
            }

            // Заполнение slack-коэффициентов
            for (int j = origN; j < n; j++)
            {
                if (j == origN + i)
                {
                    if (constraints[i].Type == ConstraintType.LowerOrEqual ||
                        constraints[i].Type == ConstraintType.Equal)
                    {
                        leftPart[j] = 1;
                    }
                    else
                    {
                        leftPart[j] = -1;
                    }
                }
                else
                {
                    leftPart[j] = 0;
                }
            }

            result[i] = new Constraint(leftPart, constraints[i].RightPart, ConstraintType.Equal);
        }

        // Добавление m+1-го уравнения при наличии ограничений-равенств
        if (m == constraints.Length + 1)
        {
            double[] leftPartSumInEquations = new double[n];
            double rightPartSumInEquations = 0;

            // Подсчёт суммы обеих частей равенств
            for (int i = 0; i < constraints.Length; i++)
            {
                if (constraints[i].Type == ConstraintType.Equal)
                {
                    for (int j = 0; j < origN; j++)
                    {
                        leftPartSumInEquations[j] += constraints[i].LeftPart[j];
                    }
                    rightPartSumInEquations += constraints[i].RightPart;

                    // Заполнение slack-коэффициентов
                    for (int j = origN; j < n; j++)
                    {
                        if (j == origN + m - 1)
                        {
                            leftPartSumInEquations[j] = -1;
                        }
                        else
                        {
                            leftPartSumInEquations[j] = 0;
                        }
                    }
                }
            }

            result[m - 1] = new Constraint(leftPartSumInEquations, rightPartSumInEquations, ConstraintType.Equal);
        }
        

        return result;
    }
}