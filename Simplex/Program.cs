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
    public static void Task1()
    {
        Console.WriteLine("--------Задание 1--------");
        const int n = 2;
        const int m = 3;

        double[] funcCoefs = new double[n] { -1, 2 };

        double[,] constraintsCoefs = new double[m, n + 1]
        {
            { 1,3, 9 },
            { 3,-2, 5 },
            { 2,1, 6 },
        };

        //MaxFunc(funcCoefs, constraintsCoefs);
    }

    public static void Task2()
    {
        Console.WriteLine("\n--------Задание 2--------");
        const int n = 2;
        const int m = 3;

        double[] funcCoefs = new double[n] { 2, -1 };

        double[,] constraintsCoefs = new double[m, n + 1]
        {
            { 3,-2, 12 },
            { -1,2, 8 },
            { -2,-1, -6 },
        };

        //MaxFunc(funcCoefs, constraintsCoefs);
    }

    public static void Main(string[] args)
    {
        var f = new TargetFunction(new double[12] { 2,5,4,5,  1,2,1,4, 3,1,5,2, }, 0);

        var c = CreateConstraintsForMongeKantarovichProblem(f.Coefs.ToArray(), new double[3] { 60, 80, 60 }, new double[4] { 50, 40, 70, 40 });

        var resultVec = LinearSolver.Optimize(f, c, minimize: true, 1000);

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
        Console.ReadKey();
    }
}