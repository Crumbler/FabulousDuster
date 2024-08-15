
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FabulousDuster.Wrappers;

public enum InputType : int {
    Mouse = 0,
    Keyboard = 1,
    Hardware = 2,
}

[Flags]
public enum MouseEvent : uint {
    Move = 0x0001,
    LeftDown = 0x0002,
    LeftUp = 0x0004,
    RightDown = 0x0008,
    RightUp = 0x0010,
    MiddleDown = 0x0020,
    MiddleUp = 0x0040,
    XDown = 0x0080,
    XUp = 0x0100,
    Wheel = 0x0800,
    VirtualDesk = 0x4000,
    Absolute = 0x8000
}

[StructLayout(LayoutKind.Sequential)]
public struct Input {
    public InputType mType;
    public InputUnion mData;
}

// Windows union equivalent structure
[StructLayout(LayoutKind.Explicit)]
public struct InputUnion {
    [FieldOffset(0)]
    public HardwareInput mHi;
    [FieldOffset(0)]
    public KeyDbInput mKi;
    [FieldOffset(0)]
    public MouseInput mMi;
}

[StructLayout(LayoutKind.Sequential)]
public struct HardwareInput {
    public uint mMsg;
    public ushort mParamL;
    public ushort mParamH;
}

[StructLayout(LayoutKind.Sequential)]
public struct KeyDbInput {
    public ushort mVk;
    public ushort mScan;
    public uint mFlags;
    public uint mTime;
    public IntPtr mExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct MouseInput {
    public int mX;
    public int mY;
    public uint mMouseData;
    public MouseEvent mFlags;
    public uint mTime;
    public IntPtr mExtraInfo;

    public readonly Input ToInput() => new() {
        mType = InputType.Mouse,
        mData = new() {
            mMi = this
        }
    };
}

public static partial class InputWrapper {
    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    private static partial uint SendInput(uint cNumInputs, in Input rInputs, int cSize);

    [SkipLocalsInit]
    public static unsafe void SendMouseInput(in MouseInput mouseInput) {
        Input inputUnion = mouseInput.ToInput();

        SendInput(1, inputUnion, sizeof(Input));
    }

    public static unsafe void SendMouseInput(in MouseInput mouseInputA, in MouseInput mouseInputB) {
        ReadOnlySpan<Input> inputs =
        [
            mouseInputA.ToInput(),
            mouseInputB.ToInput()
        ];

        SendInput(2, inputs[0], sizeof(Input));
    }

    public static unsafe void SendMouseInput(in MouseInput mouseInputA, in MouseInput mouseInputB,
        in MouseInput mouseInputC) {
        ReadOnlySpan<Input> inputs =
        [
            mouseInputA.ToInput(),
            mouseInputB.ToInput(),
            mouseInputC.ToInput()
        ];

        SendInput(3, inputs[0], sizeof(Input));
    }
}
