using System;
using System.Globalization;

namespace MaclaurinConsole;

public enum SeriesFunction
{
    Exp = 1,
    Power = 2,
    LnOnePlusX = 3,
    LnRatio = 4
}

public static class MaclaurinCalculator
{
    public static double Calculate(SeriesFunction function, double x, int n, double a = 2.0)
    {
        if (n < 0 || n > 10000)
            throw new ArgumentOutOfRangeException(nameof(n), "n должно быть от 0 до 10000.");

        return function switch
        {
            SeriesFunction.Exp => Exp(x, n),
            SeriesFunction.Power => Power(a, x, n),
            SeriesFunction.LnOnePlusX => LnOnePlusX(x, n),
            SeriesFunction.LnRatio => LnRatio(x, n),
            _ => throw new ArgumentException("Неизвестная функция.", nameof(function))
        };
    }

    // e^x = sum(x^k / k!), k = 0..n
    private static double Exp(double x, int n)
    {
        double sum = 1.0, term = 1.0;
        for (int k = 1; k <= n; k++)
        {
            term *= x / k;
            sum += term;
        }
        return sum;
    }

    // a^x = sum((x * ln(a))^k / k!), a > 0
    private static double Power(double a, double x, int n)
    {
        if (a <= 0 || double.IsNaN(a) || double.IsInfinity(a))
            throw new ArgumentOutOfRangeException(nameof(a), "Основание a должно быть положительным конечным числом.");
        return Exp(x * Math.Log(a), n);
    }

    // ln(1+x) = x - x^2/2 + x^3/3 - ...; -1 < x <= 1 (at x=1 converges conditionally)
    private static double LnOnePlusX(double x, int n)
    {
        if (x <= -1 || x > 1 || double.IsNaN(x) || double.IsInfinity(x))
            throw new ArgumentOutOfRangeException(nameof(x), "Для ряда ln(1+x) требуется -1 < x <= 1.");
        double sum = 0.0;
        for (int k = 1; k <= n; k++)
            sum += (k % 2 == 1 ? 1.0 : -1.0) * Math.Pow(x, k) / k;
        return sum;
    }

    // ln((1+x)/(1-x)) = 2 * (x + x^3/3 + x^5/5 + ...), |x| < 1
    private static double LnRatio(double x, int n)
    {
        if (Math.Abs(x) >= 1 || double.IsNaN(x) || double.IsInfinity(x))
            throw new ArgumentOutOfRangeException(nameof(x), "Для ряда ln((1+x)/(1-x)) требуется |x| < 1.");
        double sum = 0.0;
        for (int k = 0; k <= n; k++)
            sum += 2.0 * Math.Pow(x, 2 * k + 1) / (2 * k + 1);
        return sum;
    }
}

internal static class Program
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Ряды Маклорена ===");
        Console.WriteLine("1 — e^x");
        Console.WriteLine("2 — a^x (по умолчанию a = 2; можно указать своё)");
        Console.WriteLine("3 — ln(1+x)");
        Console.WriteLine("4 — ln((1+x)/(1-x))");

        if (!TryReadInt("Выберите функцию (1–4): ", out int choice) || choice < 1 || choice > 4)
        {
            Console.WriteLine("Ошибка: выберите число от 1 до 4.");
            return;
        }

        var function = (SeriesFunction)choice;
        double a = 2.0;
        if (function == SeriesFunction.Power &&
            !TryReadDouble("Введите основание a (> 0): ", out a))
        {
            Console.WriteLine("Ошибка: некорректное число.");
            return;
        }

        Console.WriteLine("1 — одно значение x");
        Console.WriteLine("2 — диапазон x");
        if (!TryReadInt("Режим: ", out int mode) || (mode != 1 && mode != 2))
        {
            Console.WriteLine("Ошибка: режим должен быть 1 или 2.");
            return;
        }

        if (!TryReadInt("Введите число членов/степень n (0–10000): ", out int n) || n < 0 || n > 10000)
        {
            Console.WriteLine("Ошибка: n должно быть от 0 до 10000.");
            return;
        }

        try
        {
            if (mode == 1)
            {
                if (!TryReadDouble("Введите x: ", out double x))
                {
                    Console.WriteLine("Ошибка: некорректное значение x.");
                    return;
                }
                PrintResult(function, x, n, a);
            }
            else
            {
                if (!TryReadDouble("Начало диапазона: ", out double start) ||
                    !TryReadDouble("Конец диапазона: ", out double end) ||
                    !TryReadDouble("Шаг (не ноль): ", out double step))
                {
                    Console.WriteLine("Ошибка: введите корректные числовые значения.");
                    return;
                }
                if (step == 0 || (end > start && step < 0) || (end < start && step > 0))
                {
                    Console.WriteLine("Ошибка: шаг должен вести от начала к концу и не быть равен нулю.");
                    return;
                }
                Console.WriteLine("       x\t\tСумма ряда\t\tЗначение Math");
                int guard = 0;
                for (double x = start; step > 0 ? x <= end + 1e-12 : x >= end - 1e-12; x += step)
                {
                    PrintResult(function, x, n, a, false);
                    if (++guard > 100000)
                    {
                        Console.WriteLine("Остановлено: слишком много точек диапазона.");
                        break;
                    }
                }
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Ошибка входных данных: {ex.Message}");
        }
    }

    private static void PrintResult(SeriesFunction function, double x, int n, double a, bool header = true)
    {
        double result = MaclaurinCalculator.Calculate(function, x, n, a);
        double? exact = function switch
        {
            SeriesFunction.Exp => Math.Exp(x),
            SeriesFunction.Power => Math.Pow(a, x),
            SeriesFunction.LnOnePlusX => Math.Log(1 + x),
            SeriesFunction.LnRatio => Math.Log((1 + x) / (1 - x)),
            _ => null
        };
        if (header) Console.WriteLine($"x = {x:G8}, n = {n}");
        Console.WriteLine($"Сумма ряда: {result:G12}");
        if (exact.HasValue) Console.WriteLine($"Значение Math: {exact.Value:G12}");
        if (header) Console.WriteLine($"Абсолютная разница: {Math.Abs(result - exact!.Value):G6}");
        else Console.WriteLine($"{x,10:G6}\t{result,16:G10}\t{exact!.Value,16:G10}");
    }

    private static bool TryReadInt(string prompt, out int value)
    {
        Console.Write(prompt);
        return int.TryParse(Console.ReadLine(), NumberStyles.Integer, Culture, out value);
    }

    private static bool TryReadDouble(string prompt, out double value)
    {
        Console.Write(prompt);
        string input = (Console.ReadLine() ?? "").Trim().Replace(',', '.');
        return double.TryParse(input, NumberStyles.Float, Culture, out value) && double.IsFinite(value);
    }
}
