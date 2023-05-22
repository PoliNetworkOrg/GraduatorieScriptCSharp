using System.Diagnostics;

namespace GraduatorieScript.Utils;

public class Metrics
{
    Stopwatch sw;
    bool stdout = true;
    public Metrics()
    {
        sw = new Stopwatch();
    }

    public void Start()
    {
        sw.Start();
    }

    public void Stop(string helper = "")
    {
        sw.Stop();
        if (stdout)
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
