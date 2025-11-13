using System.Diagnostics;
using Avalonia.Logging;

namespace Virtualization.Avalonia;

public class Log
{
#if NET9_0_OR_GREATER
    public static void Debug(string format, params Span<object?> args)
    {        
        Debugger.Log(0,"ItemRepeater",string.Format(format, args));
    }
#else
    public static void Debug(string format, params object[] args)
    {
        Debugger.Log(0, "ItemRepeater", string.Format(format, args));
        
    }
#endif
}
