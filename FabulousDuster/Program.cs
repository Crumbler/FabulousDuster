namespace FabulousDuster;

public static class Program {
    public static void Main() {
        Console.WriteLine("FabulousDuster started");

        MovementManager.AddMovementHotKeys();

        HotKeyManager.StartProcessingHotKeys();

        Thread.Sleep(Timeout.Infinite);
    }
}
