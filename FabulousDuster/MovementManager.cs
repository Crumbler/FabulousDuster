
using FabulousDuster.Helpers;
using FabulousDuster.Wrappers;

namespace FabulousDuster;

public static class MovementManager {
    private static bool _isMoving,
        _isShuttingDown;
    private static CancellationTokenSource? _movementCancelSource;

    public static void AddMovementHotKeys() {
        HotKeyManager.AddHotKey(KeyModifiers.Control | KeyModifiers.Alt, Keys.B, OnMovementHotkey);
        HotKeyManager.AddHotKey(KeyModifiers.Control | KeyModifiers.Alt, Keys.N, OnShutdownHotkey);

        Console.WriteLine("Movement hotkeys added");
    }

    private static void OnMovementHotkey() {
        if (_isShuttingDown) {
            Console.WriteLine("Cannot start movement while shutting down");
            return;
        }

        _isMoving = !_isMoving;

        Console.WriteLine("Movement " + (_isMoving ? "enabled" : "disabled"));

        if (!_isMoving) {
            _movementCancelSource?.Cancel();
            return;
        }

        _movementCancelSource = new CancellationTokenSource();
        var movementThread = new Thread(MovementFunc) {
            IsBackground = true
        };
        movementThread.Start(_movementCancelSource.Token);
    }

    private static void MovementFunc(object? cancellationTokenObj) {
        ArgumentNullException.ThrowIfNull(cancellationTokenObj);
        var cancellationToken = (CancellationToken)cancellationTokenObj;

        if (!CursorWrapper.GetCursorPos(out POINT cursorPos)) {
            throw new Exception("Failed to get cursor position");
        }

        Console.WriteLine("Cursor position: " + cursorPos);

        POINT leftPos = cursorPos with {
            X = cursorPos.X - 50
        },
        rightPos = cursorPos with {
            X = cursorPos.X + 50
        };

        do {
            Thread.Sleep(5000);
            if (cancellationToken.IsCancellationRequested) {
                break;
            }

            MouseHelper.MoveToPointAndClick(leftPos);

            Thread.Sleep(5000);
            if (cancellationToken.IsCancellationRequested) {
                break;
            }

            MouseHelper.MoveToPointAndClick(rightPos);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private static void OnShutdownHotkey() {
        if (_isShuttingDown) {
            Console.WriteLine("Cannot start movement while shutting down");
            return;
        }

        _isShuttingDown = !_isShuttingDown;

        Console.WriteLine("Shutdown " + (_isShuttingDown ? "enabled" : "disabled"));
    }
}
