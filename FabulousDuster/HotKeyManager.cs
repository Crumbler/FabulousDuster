
using static FabulousDuster.MessageWrapper;

namespace FabulousDuster;

public static class HotKeyManager {
    private static int _isProcessingHotkeys;
    private static readonly Dictionary<int, Action> _hotKeyActions = [];

    public static void AddHotKey(KeyModifiers modifiers, Keys key, Action action) {
        int id = _hotKeyActions.Count;

        if (!HotKeyWrapper.RegisterHotKey(0, id, modifiers, key)) {
            throw new Exception($"Failed to register hotkey for key {key} with modifier {modifiers}");
        }

        _hotKeyActions.Add(id, action);
    }

    public static void StartProcessingHotKeys() {
        const int initialValue = 0;
        if (initialValue != Interlocked.CompareExchange(ref _isProcessingHotkeys, 1, 0)) {
            throw new Exception("Only one hotkey processing thread may run at a time");
        }

        var readHotKeysThread = new Thread(ProcessHotKeys) {
            IsBackground = true
        };

        readHotKeysThread.Start();
    }

    private static void ProcessHotKeys() {
        CreateMessageQueue();

        Console.WriteLine("Reading hotkeys");

        AddHotKey(KeyModifiers.Control | KeyModifiers.Alt, Keys.B,
            static () => Console.WriteLine("Pressed B"));

        while (true) {
            if (!GetMessage(out MSG msg, 0, MessageTypes.HotKey, MessageTypes.HotKey)) {
                continue;
            }

            int hotkeyId = (int)msg.wParam;

            // System hotkey
            if (hotkeyId == -1 || hotkeyId == -2) {
                continue;
            }

            Console.WriteLine("Received hotkey with id: " + hotkeyId);

            _hotKeyActions[hotkeyId]();
        }
    }

    private static void CreateMessageQueue() {
        PeekMessage(out _, 0, 0, 0, 0);

        uint currThreadId = ThreadWrapper.GetCurrentThreadId();

        // User message
        PostThreadMessage(currThreadId, MessageTypes.UserMessage, 0, 0);

        if (!GetMessage(out _, 0, MessageTypes.UserMessage, MessageTypes.UserMessage)) {
            throw new Exception("Failed to create message queue");
        }
    }
}
