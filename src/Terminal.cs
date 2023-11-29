using System.Runtime.InteropServices;

namespace NxScript;

internal static class Terminal
{
    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern uint GetLastError();

    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;

    public static void EnableVirtualTerminalOutput()
    {
        // Added in windows 10 https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
        var stdoutHandle = GetStdHandle(StdOutputHandle);
        _ = GetConsoleMode(stdoutHandle, out var outConsoleMode);
        outConsoleMode |= EnableVirtualTerminalProcessing;
        _ = SetConsoleMode(stdoutHandle, outConsoleMode);
    }

    public static void Error(string message)
    {
        Console.WriteLine("ERROR: ".InRed() + message);
    }

    public static void Info(string message)
    {
        Console.WriteLine("INFO:  ".InBlue() + message);
    }

    public static void Separate()
    {
        Console.WriteLine(new string('-', Console.BufferWidth -1));
    }
}