﻿using Inputs.Misc;

using System.Diagnostics;

namespace Inputs.InputMethods.Keyboard;

/// <summary>
/// A keyboard input method utilizing the deprecated 'keybd_event' win32 function.
/// </summary>
public sealed class KeyboardEvent : IKeyboardInput {
    public string Name => nameof(KeyboardEvent);

    private readonly List<VK> heldKeys = [];

    public bool Press(VK key) {
        if (heldKeys.Contains(key))
            return true;

        try {
            Native.User32.keybd_event((byte)key, 0, Native.User32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);

            heldKeys.Add(key);

            return true;
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }

        return false;
    }

    public bool Release(VK key) {
        if (!heldKeys.Contains(key))
            return true;

        try {
            Native.User32.keybd_event((byte)key, 0, Native.User32.KEYEVENTF_KEYUP, UIntPtr.Zero);

            heldKeys.Remove(key);

            return true;
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }

        return false;
    }

    public void Dispose() {
        foreach (var key in heldKeys) {
            Native.User32.keybd_event((byte)key, 0, Native.User32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }

    ~KeyboardEvent() => Dispose();
}
