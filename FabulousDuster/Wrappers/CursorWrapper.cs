
using System.Runtime.InteropServices;

namespace FabulousDuster.Wrappers; 
public static partial class CursorWrapper {
    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(out POINT point);
}
