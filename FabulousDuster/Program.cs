using Inputs;
using Inputs.InputMethods.Mouse;

namespace FabulousDuster;

public static class Program {
    public static void Main() {
        Console.WriteLine("FabulousDuster started");

        Mouse.SetMethodFrom<NtUserSendInput>();

        MovementManager.AddMovementHotKeys();

        HotKeyManager.StartProcessingHotKeys();

        Thread.Sleep(Timeout.Infinite);
    }
}
