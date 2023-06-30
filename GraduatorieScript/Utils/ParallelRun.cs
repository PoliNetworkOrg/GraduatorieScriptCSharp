﻿using System.Diagnostics;

namespace GraduatorieScript.Utils;

public static class ParallelRun
{
    private static readonly int MaxThreadCount = Process.GetCurrentProcess().MaxWorkingSet.ToInt32();
    private const int Max = 50;
    private static readonly int MaxCoreCount = Environment.ProcessorCount;

    public static void Run(Action[] toArray)
    {
        var maxDegreeOfParallelism = Math.Min(MaxCoreCount, Math.Min(Max, MaxThreadCount));
        var parallelOptions = new ParallelOptions(){MaxDegreeOfParallelism = maxDegreeOfParallelism};
        Parallel.Invoke(parallelOptions: parallelOptions, actions:toArray);
    }
}