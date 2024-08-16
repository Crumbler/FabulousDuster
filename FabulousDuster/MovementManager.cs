
using FabulousDuster.Helpers;
using FabulousDuster.Wrappers;

using Inputs;

using System.Globalization;

namespace FabulousDuster;

public static class MovementManager {
    private static bool _isMoving,
        _isShuttingDown;
    private static CancellationTokenSource? _movementCancelSource,
        _shutdownCancelSource;

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

            //MouseHelper.MoveToPointAndClick(leftPos);
            Mouse.SetCursorPos(leftPos.X, leftPos.Y);
            Mouse.Click(MouseKey.Left, 0);

            await Task.Delay(5000, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            //MouseHelper.MoveToPointAndClick(rightPos);
            Mouse.SetCursorPos(rightPos.X, rightPos.Y);
            Mouse.Click(MouseKey.Left, 0);
        }
        while (!cancellationToken.IsCancellationRequested);

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static void OnShutdownHotkey() {
        if (_isMoving) {
            Console.WriteLine("Cannot start shutdown while moving");
            return;
        }

        _isShuttingDown = !_isShuttingDown;

        if (_isShuttingDown) {
            _shutdownCancelSource?.Dispose();
            _shutdownCancelSource = new CancellationTokenSource();
            ShutdownFunc(_shutdownCancelSource.Token).ContinueWith((t, args) => {
                if (t.IsCanceled) {
                    Console.WriteLine("Shutdown canceled");
                }
            }, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
        } else {
            _shutdownCancelSource?.Cancel();
        }

        Console.WriteLine("Shutdown " + (_isShuttingDown ? "enabled" : "disabled"));
    }

    private static async Task ShutdownFunc(CancellationToken cancellationToken) {
        if (!CursorWrapper.GetCursorPos(out POINT cursorPos)) {
            throw new Exception("Failed to get cursor position");
        }

        Console.WriteLine("Please enter the shutdown hour");
        int hour = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.WriteLine("Please enter the shutdown minute");
        int minute = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.WriteLine("Shutdown based off of cursor position: " + cursorPos);

        POINT leftPos = cursorPos with {
            X = cursorPos.X - 50,
            Y = cursorPos.Y - 80
        },
        rightPos = cursorPos with {
            X = cursorPos.X + 50,
            Y = cursorPos.Y - 80
        };

        do {
            await Task.Delay(5000, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            MouseHelper.MoveToPointAndClick(leftPos);

            await Task.Delay(5000, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            MouseHelper.MoveToPointAndClick(rightPos);

            var timeNow = DateTime.Now;

            if (timeNow.Hour >= hour && timeNow.Minute > minute) {
                _isShuttingDown = false;
                MouseHelper.MoveToPointAndClick(cursorPos);

                Console.WriteLine("Shutting down due to time");
                return;
            }
        }
        while (!cancellationToken.IsCancellationRequested);

        cancellationToken.ThrowIfCancellationRequested();
    }
}
