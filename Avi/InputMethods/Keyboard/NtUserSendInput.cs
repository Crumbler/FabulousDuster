using Inputs.Misc;

using System.Diagnostics;
using System.Runtime.InteropServices;

using static Inputs.Misc.Native.Kernel32;

namespace Inputs.InputMethods.Keyboard;

public sealed class NtUserSendInput : IKeyboardInput {
    public string Name => nameof(NtUserSendInput);

    public NtUserSendInput() {
        if (NtUserSendInputBytes.Length == 0) {
            var temp = Misc.Help.GetUnmanagedFunctionBytes("win32u", "NtUserSendInput");
            if (temp.Length > 0)
                NtUserSendInputBytes = crypto.Encrypt(temp);
        }
    }

    private readonly List<VK> heldKeys = [];
    private static readonly XoR crypto = new();
    private static byte[] NtUserSendInputBytes = [];
    private delegate void _NtUserSendInput(uint cInputs, IntPtr input, int cbSize);

    private bool Call(VK key, ScanCodeShort code, Native.User32.KEYEVENTF dwFlags, int time, UIntPtr dwExtraInfo) {
        try {
            // same story
            if (NtUserSendInputBytes.Length == 0) {
                NtUserSendInputBytes = Misc.Help.GetUnmanagedFunctionBytes("win32u", "NtUserSendInput");

                if (NtUserSendInputBytes.Length == 0) {
                    Debug.WriteLine("Failed to get NtUserSendInputBytes bytes");
                    return false;
                }

                NtUserSendInputBytes = crypto.Encrypt(NtUserSendInputBytes);
            }

            // Alloc the bytes
            IntPtr addy = VirtualAlloc(IntPtr.Zero, (uint)NtUserSendInputBytes.Length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            // Copy the bytes to the address
            Marshal.Copy(crypto.Decrypt(NtUserSendInputBytes), 0, addy, NtUserSendInputBytes.Length);

            // Create input struct
            Native.User32.INPUT input = new() {
                type = 1 // keyboard input  
            };
            input.U.ki.time = time;
            input.U.ki.dwExtraInfo = dwExtraInfo;
            input.U.ki.dwFlags = dwFlags;
            input.U.ki.wVk = key;
            input.U.ki.wScan = code;

            IntPtr inputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(input));
            Marshal.StructureToPtr(input, inputPtr, true);

            // Create a delegate for the memory chunk & execute it 
            ((_NtUserSendInput)Marshal.GetDelegateForFunctionPointer(addy, typeof(_NtUserSendInput)))(1u, inputPtr, Marshal.SizeOf(input));

            // Free the memory
            VirtualFree(addy, NtUserSendInputBytes.Length, FreeType.Release);

            return true;
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
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }
    }

    ~NtUserSendInput() => Dispose();
}
