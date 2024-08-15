
using FabulousDuster.Wrappers;

namespace FabulousDuster.Helpers; 

public static class MouseHelper {
    private static readonly Rectangle _screenBounds = Screen.PrimaryScreen!.Bounds;

    private static MouseInput CreateMovementInput(POINT point) {
        // Coordinates have to be normalized between 0 and 65535
        int x = (int)MathF.Round(point.X / (float)_screenBounds.Width * ushort.MaxValue);
        int y = (int)MathF.Round(point.Y / (float)_screenBounds.Height * ushort.MaxValue);

        MouseInput movementInput = new() {
            mFlags = MouseEvent.Move | MouseEvent.Absolute,
            mX = x,
            mY = y
        };

        return movementInput;
    }

    public static void MoveToPoint(POINT point) {
        MouseInput movementInput = CreateMovementInput(point);

        InputWrapper.SendMouseInput(movementInput);
    }

    public static void MoveToPointAndClick(POINT point) {
        MouseInput movementInput = CreateMovementInput(point);

        MouseInput mouseDownInput = new() {
            mFlags = MouseEvent.LeftDown
        };
        MouseInput mouseUpInput = new() {
            mFlags = MouseEvent.LeftUp
        };

        InputWrapper.SendMouseInput(movementInput, mouseDownInput, mouseUpInput);
    }

    public static void Click() {
        MouseInput mouseDownInput = new() {
            mFlags = MouseEvent.LeftDown
        };
        MouseInput mouseUpInput = new() {
            mFlags = MouseEvent.LeftUp
        };

        InputWrapper.SendMouseInput(mouseDownInput, mouseUpInput);
    }
}
