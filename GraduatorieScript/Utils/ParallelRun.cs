using System.Diagnostics;

namespace GraduatorieScript.Utils;

public static class ParallelRun
{
    private const int MaxWithoutDebug = 50;
    private static readonly int MaxWithDebug = System.Diagnostics.Debugger.IsAttached ? 1 : MaxWithoutDebug;
    private static readonly int MaxCoreCount = Environment.ProcessorCount;

    public static void Run(Action[] toArray)
    {
        var maxDegreeOfParallelism = Math.Max(Math.Min(MaxCoreCount, MaxWithDebug), 1);
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        Parallel.Invoke(parallelOptions, toArray);
    }
}