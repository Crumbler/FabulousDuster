
using static Inputs.Misc.Native.User32;

namespace Inputs.Misc {
    public static class InputExtensions {
        internal static MOUSEEVENTF_FLAGS MapMouseKey(this MouseKey key, bool down) {
            MOUSEEVENTF_FLAGS keyType;

            if (down) {
                keyType = key switch {
                    MouseKey.Left => MOUSEEVENTF_FLAGS.MOUSEEVENTF_LEFTDOWN,
                    MouseKey.Right => MOUSEEVENTF_FLAGS.MOUSEEVENTF_RIGHTDOWN,
                    MouseKey.Middle => MOUSEEVENTF_FLAGS.MOUSEEVENTF_MIDDLEDOWN,
                    _ => throw new NotSupportedException("Unsupported Key Type."),
                };
            } else {
                keyType = key switch {
                    MouseKey.Left => MOUSEEVENTF_FLAGS.MOUSEEVENTF_LEFTUP,
                    MouseKey.Right => MOUSEEVENTF_FLAGS.MOUSEEVENTF_RIGHTUP,
                    MouseKey.Middle => MOUSEEVENTF_FLAGS.MOUSEEVENTF_MIDDLEUP,
                    _ => throw new NotSupportedException("Unsupported Key Type."),
                };
            }

            return keyType;
        }
    }
}