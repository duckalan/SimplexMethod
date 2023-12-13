using Simplex;

public class Program
{
    public static Constraint[] CreateConstraintsForMongeKantarovichProblem(double[] funcCoefs, double[] reservs, double[] needs)
    {
        var result = new Constraint[reservs.Length + needs.Length];

        for (int i = 0; i < reservs.Length; i++)
        {
            var constraintCoefs = new double[funcCoefs.Length];

            for(int j = 0; j < needs.Length; j++)
            {
                constraintCoefs[i * needs.Length + j] = 1;
            }

            result[i] = new Constraint(constraintCoefs, reservs[i], ConstraintType.Equal);
        }

        for (int i = 0; i < needs.Length; i++)
        {
            var constraintCoefs = new double[funcCoefs.Length];

            for (int j = 0; j < reservs.Length; j++)
            {
                constraintCoefs[i + j * needs.Length] = 1;
            }

            result[i + reservs.Length] = new Constraint(constraintCoefs, needs[i], ConstraintType.Equal);
        }

        return result;
    }

    public static void SolveTask(TargetFunction f, Constraint[] c)
    {
        var resultVec = LinearSolver.Optimize(f, c, minimize: false, 1000);

        double sum = resultVec.Zip(f.Coefs.ToArray(), (a, b) => a * b).Sum();

        Console.WriteLine("Оптимальное решение:");
        for (int i = 0; i < resultVec.Length; i++)
        {
            Console.Write($"x{i + 1} = {Math.Round(resultVec[i], 2)}; ");
        }
        Console.WriteLine();
        Console.WriteLine();

        Console.Write("z = ");
        for (int i = 0; i < resultVec.Length; i++)
        {
            Console.Write($"{Math.Round(resultVec[i], 2)}*({f.Coefs[i]})");
            if (i != resultVec.Length - 1)
            {
                Console.Write(" + ");
            }
        }
        Console.WriteLine($" = {sum,5:F2}");
    }

    public static void Main(string[] args)
    {
        var f1 = new TargetFunction(new double[] { 1, 2 }, 0);
        var c1 = new Constraint[]
        {
            new Constraint(new double[] {1, 3}, 9, ConstraintType.LowerOrEqual),
            new Constraint(new double[] {3, -2}, 5, ConstraintType.LowerOrEqual),
            new Constraint(new double[] {2, 1}, 6, ConstraintType.LowerOrEqual),
        };

        Console.WriteLine("\nЗадание 1");
        SolveTask(f1, c1);

        var f2 = new TargetFunction(new double[] { 2, -1 }, 0);
        var c2 = new Constraint[]
        {
            new Constraint(new double[] {3, -2}, 12, ConstraintType.LowerOrEqual),
            new Constraint(new double[] {-1, 2}, 8, ConstraintType.LowerOrEqual),
            new Constraint(new double[] {2, 3}, 6, ConstraintType.GreaterOrEqual),
        };

        Console.WriteLine("\nЗадание 2");
        SolveTask(f2, c2);
        Console.ReadKey();
    }
}