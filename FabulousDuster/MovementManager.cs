
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

        _movementCancelSource?.Dispose();
        _movementCancelSource = new CancellationTokenSource();
        MovementFunc(_movementCancelSource.Token).ContinueWith((t, args) => {
            Console.WriteLine("Movement canceled");
        }, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
    }

    private static async Task MovementFunc(CancellationToken cancellationToken) {
        if (!CursorWrapper.GetCursorPos(out POINT cursorPos)) {
            throw new Exception("Failed to get cursor position");
        }

        Console.WriteLine("Movement based off of cursor position: " + cursorPos);

        POINT leftPos = cursorPos with {
            X = cursorPos.X - 50
        },
        rightPos = cursorPos with {
            X = cursorPos.X + 50
        };

        do {
            await Task.Delay(5000, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            MouseHelper.MoveToPointAndClick(leftPos);

            await Task.Delay(5000, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            MouseHelper.MoveToPointAndClick(rightPos);
        }
        while (!cancellationToken.IsCancellationRequested);

        cancellationToken.ThrowIfCancellationRequested();
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
