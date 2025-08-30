using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Tools;

public static class Should
{
    public static void ShouldBe<T>(this T? actual, T? expected, IEqualityComparer<T>? comparer = null)
    {
        var test = (comparer ?? EqualityComparer<T>.Default).Equals(actual, expected);
        if (!test)
        {
            throw new InvalidOperationException($"Expected '{expected}', but was '{actual}'.");
        }
    }

    public static void ShouldBe<T>(this T[] actual, T[] expected, IEqualityComparer<T>? comparer = null)
    {
        var test = actual.Length == expected.Length;
        if (test)
        {
            comparer ??= EqualityComparer<T>.Default;
            for (var i = 0; i < actual.Length; i++)
            {
                if (!comparer.Equals(actual[i], expected[i]))
                {
                    test = false;
                    break;
                }
            }
        }

        if (!test)
        {
            throw new InvalidOperationException($"Expected '{string.Join("; ", expected)}', but was '{string.Join("; ", actual)}'.");
        }
    }

    public static void ShouldContain(this string actual, string expected)
    {
        if (!actual.Contains(expected))
        {
            throw new InvalidOperationException($"Expected '{actual}', contains '{expected}'.");
        }
    }

    public static async Task<T> ThrowAsync<T>(Func<Task> action)
        where T : Exception
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            if (ex is T expected)
            {
                return expected;
            }

            throw new InvalidOperationException($"Should throw '{typeof(T).Name}', but did '{ex.GetType().Name}'.", ex);
        }

        throw new InvalidOperationException($"Should throw '{typeof(T).Name}', but did not.");
    }
}