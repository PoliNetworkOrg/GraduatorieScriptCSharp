using System.Diagnostics;

namespace GraduatorieScript.Utils;

public class Metrics
{
    private const bool Stdout = true;
    private readonly Stopwatch sw;

    public Metrics()
    {
        sw = new Stopwatch();
    }

    private void Start()
    {
        sw.Start();
    }

    private void Stop(string helper = "")
    {
        sw.Stop();
        if (Stdout)
        {
            var ms = sw.ElapsedMilliseconds;
            var helperMsg = helper == "" ? "" : $" {helper}:";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[Metrics]{helperMsg} {ms}ms");
            Console.ResetColor();
        }

        sw.Reset();
    }

    public T Execute<T>(Func<T> func, string helper = "")
    {
        var fullFuncName = func.Method.DeclaringType?.FullName + "." + func.Method.Name;

        Start();

        var result = func.Invoke();

        Stop(helper == "" ? fullFuncName : helper);
        return result;
    }
}