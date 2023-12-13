namespace Simplex;

public class LinearSolver
{
    public static bool IsOptimized(Tableau tableau)
    {
        for (int j = 0; j < tableau.FuncRow.Length - 1; j++)
        {
            if (tableau.FuncRow[j] < 0)
            {
                return false;
            }
        }

        return true;
    }

    public static double[] Optimize(TargetFunction f, Constraint[] constraints, bool minimize, int maxIterationsCount)
    {
        // Проверки на одинаковое количество переменных в ограничениях
        // и функции
        for (int i = 0; i < constraints.Length; i++)
        {
            for (int j = i + 1; j < constraints.Length; j++)
            {
                if (constraints[i].LeftPart.Length != constraints[j].LeftPart.Length)
                {
                    throw new Exception();
                }
            }
        }
        if (f.Coefs.Length != constraints[0].LeftPart.Length)
        {
            throw new Exception();
        }

        var canonicalConstraints = constraints.ToCanonicalForm();

        var funcCoefs = new double[f.Coefs.Length];
        var constant = f.Constant;
        if (minimize)
        {
            for (int i = 0; i < f.Coefs.Length; i++)
            {
                funcCoefs[i] = f.Coefs[i] * -1;
            }
            constant *= -1;
        }
        else
        {
            for (int i = 0; i < f.Coefs.Length; i++)
            {
                funcCoefs[i] = f.Coefs[i];
            }
        }
        var func = new TargetFunction(funcCoefs, constant);

        int artificalVarsCount = 0;
        for (int i = 0; i < canonicalConstraints.Length; i++)
        {
            int j = f.Coefs.Length + i;
            if (canonicalConstraints[i].LeftPart[j] == -1)
            {
                artificalVarsCount++;
            }
        }

        double[] optimalSolution;

        if (artificalVarsCount != 0)
        {
            var tableau = CalculateArtificalBasis(func, canonicalConstraints, artificalVarsCount);
            Console.WriteLine("Симплекс таблица после метода искусственного базиса:");
            Console.WriteLine(tableau);
            optimalSolution = SimplexMethod(tableau, maxIterationsCount);
        }
        else
        {
            var tableau = new Tableau(func, canonicalConstraints);
            Console.WriteLine("Исходная симплекс таблица:");
            Console.WriteLine(tableau);
            optimalSolution = SimplexMethod(tableau, maxIterationsCount);
        }

        var result = new double[f.Coefs.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = optimalSolution[i];
        }


        return result;
    }

    private static Tableau CalculateArtificalBasis(TargetFunction f, Constraint[] canonicalConstraints, int
        artificalVarsCount)
    {
        int origN = canonicalConstraints[0].LeftPart.Length;
        int n = origN + artificalVarsCount;
        const double M = 500;

        // Искусственные переменные, выраженные через другие переменные
        var artificalVarsCoefs = new double[artificalVarsCount, origN + 1];
        var newConstraints = new Constraint[canonicalConstraints.Length];
        int t = 0;
        // Вводим искусственные переменные
        for (int i = 0; i < canonicalConstraints.Length; i++)
        {
            var leftPart = new double[n];

            for (int w = 0; w < canonicalConstraints[0].LeftPart.Length; w++)
            {
                leftPart[w] = canonicalConstraints[i].LeftPart[w];
            }

            int j = f.Coefs.Length + i;
            if (canonicalConstraints[i].LeftPart[j] == -1)
            {
                leftPart[origN + t] = 1;

                artificalVarsCoefs[t, 0] = canonicalConstraints[i].RightPart;
                for (int w = 1; w < origN + 1; w++)
                {
                    // Тут правая часть [0], остальное x1-x4
                    artificalVarsCoefs[t, w] = -canonicalConstraints[i].LeftPart[w - 1] * M;
                }

                t++;
            }

            newConstraints[i] = new Constraint(leftPart, canonicalConstraints[i].RightPart, ConstraintType.Equal);
        }

        // Складываем коэффициенты исходной функции
        // с выраженными искусственными переменными
        var finedFunc = new double[n];
        for (int i = 0; i < f.Coefs.Length; i++)
        {
            finedFunc[i] = f.Coefs[i];
        }

        double newConstant = f.Constant;
        for (int i = 0; i < artificalVarsCount; i++)
        {
            for (int j = 1; j < origN + 1; j++)
            {
                finedFunc[j - 1] -= artificalVarsCoefs[i, j];
            }
            newConstant -= artificalVarsCoefs[i, 0] * M;
        }

        var newF = new TargetFunction(finedFunc, newConstant);

        return new Tableau(newF, newConstraints);
    }


    private static double[] SimplexMethod(Tableau tableau, int maxIterationsCount)
    {
        int iterationCount = 0;
        while (!IsOptimized(tableau) && iterationCount < maxIterationsCount)
        {
            // Ищем индекс ведущего столбца
            double min = double.MaxValue;
            int minColIndex = 0;

            // Для работы с искусственным базисом tableau.Width - 1
            for (int j = 0; j < tableau.Width - 1; j++)
            {
                if (tableau[tableau.Height - 1, j] < min)
                {
                    min = tableau[tableau.Height - 1, j];
                    minColIndex = j;
                }
            }

            // Делим столбец свободных коэффициентов на значения из ведущего столбца
            // и находим индекс ведущей строки
            var tempMat = new double[tableau.Height, tableau.Width];
            for (int i = 0; i < tableau.Height; i++)
            {
                for (int j = 0; j < tableau.Width; j++)
                {
                    tempMat[i, j] = tableau[i, j];
                }
            }
            min = double.MaxValue;
            int minRowIndex = 0;

            for (int i = 0; i < tableau.Height; i++)
            {
                tempMat[i, tempMat.GetLength(1) - 1] /= tableau[i, minColIndex];

                if (tempMat[i, tempMat.GetLength(1) - 1] >= 0
                    && tempMat[i, minColIndex] > 0)
                {
                    if (tempMat[i, tempMat.GetLength(1) - 1] < min)
                    {
                        min = tempMat[i, tableau.Width - 1];
                        minRowIndex = i;
                    }
                }

            }

            // Pivoting
            // Делим ведущую строку на ведущий элемент
            double pivot = tableau[minRowIndex, minColIndex];
            for (int j = 0; j < tableau.Width; j++)
            {
                tableau[minRowIndex, j] /= pivot;
            }

            // Обнуляем строки над ведущим элементом
            for (int i = 0; i < minRowIndex; i++)
            {
                double divider = tableau[i, minColIndex];

                // Вычитание из строки ведущей строки, умноженной на элемент из ведущего столбца
                for (int j = 0; j < tableau.Width; j++)
                {
                    tableau[i, j] -= tableau[minRowIndex, j] * divider;
                }
            }

            // Обнуляем строки под ведущим элементом
            for (int i = minRowIndex + 1; i < tableau.Height; i++)
            {
                double divider = tableau[i, minColIndex];

                // Вычитание из строки ведущей строки, умноженной на элемент из ведущего столбца
                for (int j = 0; j < tableau.Width; j++)
                {
                    tableau[i, j] -= tableau[minRowIndex, j] * divider;
                }
            }

            iterationCount++;
        }

        if (iterationCount == maxIterationsCount)
        {
            Console.WriteLine($"Оптимальное решение не было найдено за {maxIterationsCount} итераций");
        }

        // БРАТЬ ТОЛЬКО ИЗ ЕДИНИЧНЫХ СТОЛБЦОВ
        var result = new double[tableau.OrigVarsCount];
        for (int j = 0; j < tableau.OrigVarsCount; j++)
        {
            if (IsUnitColumn(j, tableau))
            {
                for (int i = 0; i < tableau.Height; i++)
                {
                    if (tableau[i, j] == 1)
                    {
                        result[j] = tableau[i, tableau.Width - 1];
                    }
                }
            }
        }

        Console.WriteLine("Финальная таблица");
        Console.WriteLine(tableau);
        return result;
    }

    private static bool IsUnitColumn(int colIndex, Tableau tableau)
    {
        int unitCount = 0;
        for (int i = 0; i < tableau.Height; i++)
        {
            if (tableau[i, colIndex] == 1)
            {
                unitCount++;
            }
        }

        return unitCount == 1;
    }
}
