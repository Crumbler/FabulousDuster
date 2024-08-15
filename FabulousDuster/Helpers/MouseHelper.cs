
using FabulousDuster.Wrappers;

namespace FabulousDuster.Helpers; 

public static class MouseHelper {
    private static readonly Rectangle _screenBounds = Screen.PrimaryScreen!.Bounds;

    public static void MoveToPoint(POINT point) {
        int x = (int)MathF.Round(point.X / (float)_screenBounds.Width * ushort.MaxValue);
        int y = (int)MathF.Round(point.Y / (float)_screenBounds.Height * ushort.MaxValue);

        MouseInput input = new() {
            mFlags = MouseEvent.Move | MouseEvent.Absolute,
            mX = x,
            mY = y
        };

        InputWrapper.SendMouseInput(input);
    }

    public static void MoveToPointAndClick(POINT point) {
        int x = (int)MathF.Round(point.X / (float)_screenBounds.Width * ushort.MaxValue);
        int y = (int)MathF.Round(point.Y / (float)_screenBounds.Height * ushort.MaxValue);

        MouseInput inputA = new() {
            mFlags = MouseEvent.Move | MouseEvent.Absolute,
            mX = x,
            mY = y
        };
        MouseInput inputB = new() {
            mFlags = MouseEvent.LeftDown
        };
        MouseInput inputC = new() {
            mFlags = MouseEvent.LeftUp
        };

        InputWrapper.SendMouseInput(inputA, inputB, inputC);
    }

    public static void Click() {
        MouseInput inputA = new() {
            mFlags = MouseEvent.LeftDown
        };
        MouseInput inputB = new() {
            mFlags = MouseEvent.LeftUp
        };

        InputWrapper.SendMouseInput(inputA, inputB);
    }
}
