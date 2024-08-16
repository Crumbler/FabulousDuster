using Inputs.Misc;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Inputs.InputMethods.Keyboard;

public sealed class NtUserInjectKeyboardInput : IKeyboardInput {
    public string Name => nameof(NtUserInjectKeyboardInput);

    private readonly List<VK> heldKeys = [];
    private delegate void _NtUserInjectKeyboardInput(IntPtr input, int n);
    private IntPtr handle = IntPtr.Zero;

    public NtUserInjectKeyboardInput() {
        handle = Native.Kernel32.LoadLibrary("win32u.dll");
    }

    private bool Call(VK key, ScanCodeShort code, Native.User32.KEYEVENTF dwFlags, int time, UIntPtr dwExtraInfo) {
        try {
            if (handle == IntPtr.Zero)
                handle = Native.Kernel32.LoadLibrary("win32u.dll");

            if (handle == IntPtr.Zero) {
                Debug.WriteLine("Failed to load win32u.dll");
                return false;
            }

            try {
                IntPtr address = Native.Kernel32.GetProcAddress(handle, "NtUserInjectKeyboardInput");

                if (address == IntPtr.Zero) {
                    Debug.WriteLine("Failed to get Address for the 'NtUserInjectKeyboardInput'-function.");
                    return false;
                }

                unsafe {
                    *(void**)&address = (void*)address;
                }

                // Create input struct
                Native.User32.KEYBDINPUT input = new() {
                    time = time,
                    dwExtraInfo = dwExtraInfo,
                    dwFlags = dwFlags,
                    wVk = key,
                    wScan = code
                };

                IntPtr inputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(input));
                Marshal.StructureToPtr(input, inputPtr, true);

                ((_NtUserInjectKeyboardInput)Marshal.GetDelegateForFunctionPointer(address, typeof(_NtUserInjectKeyboardInput)))(inputPtr, 1);

                return true;
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }

        return false;
    }

    public bool Press(VK key) {
        if (heldKeys.Contains(key))
            return true;

        try {
            var result = Call(key, 0, Native.User32.KEYEVENTF.KEYDOWN, 0, UIntPtr.Zero);

            heldKeys.Add(key);

            return result;
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }

        return false;
    }

    public bool Release(VK key) {
        if (!heldKeys.Contains(key))
            return true;

        try {
            var result = Call(key, 0, Native.User32.KEYEVENTF.KEYUP, 0, UIntPtr.Zero);

            heldKeys.Remove(key);

            return result;
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }

        return false;
    }

    public void Dispose() {
        try {
            foreach (var key in heldKeys) {
                Call(key, 0, Native.User32.KEYEVENTF.KEYUP, 0, UIntPtr.Zero);
            }

            Native.Kernel32.FreeLibrary(handle);
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }
    }

    ~NtUserInjectKeyboardInput() => Dispose();
}
