using System.Diagnostics;

namespace GraduatorieScript.Utils;

public static class ParallelRun
{
    private const int MaxWithoutDebug = 50;
    private static readonly int MaxWithDebug = Debugger.IsAttached ? 1 : MaxWithoutDebug;
    private static readonly int MaxCoreCount = Environment.ProcessorCount;
    public static readonly int MaxDegreeOfParallelism = Math.Max(Math.Min(MaxCoreCount, MaxWithDebug), 1);

    public static void Run(Action[] toArray)
    {
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };
        Parallel.Invoke(parallelOptions, toArray);
    }
}