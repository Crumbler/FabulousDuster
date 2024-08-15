

using System.Runtime.InteropServices;

namespace FabulousDuster; 
public static partial class ThreadWrapper {
    [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]

    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static partial uint GetCurrentThreadId();
}
