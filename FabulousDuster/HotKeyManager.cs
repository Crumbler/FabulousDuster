
using System.Windows.Forms;
using static FabulousDuster.MessageWrapper;

namespace FabulousDuster;

public static class HotKeyManager {
    private static int _isProcessingHotkeys;
    private static readonly List<(KeyModifiers modifiers, Keys key, Action action)> _hotkeys = [];

    public static void AddHotKey(KeyModifiers modifiers, Keys key, Action action) {
        _hotkeys.Add((modifiers, key, action));
    }

    private static void AddHotKeys() {
        for (int i = 0; i < _hotkeys.Count; ++i) {
            var hotkey = _hotkeys[i];
            if (!HotKeyWrapper.RegisterHotKey(0, i, hotkey.modifiers, hotkey.key)) {
                throw new Exception($"Failed to register hotkey for key {hotkey.key} with modifier {hotkey.modifiers}");
            }
        }
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

        AddHotKeys();

        Console.WriteLine("Reading hotkeys");

        while (true) {
            if (!GetMessage(out MSG msg, 0, MessageTypes.HotKey, MessageTypes.HotKey)) {
                continue;
            }

            int hotkeyId = (int)msg.wParam;

            // System hotkey
            if (hotkeyId == -1 || hotkeyId == -2) {
                continue;
            }

            //Console.WriteLine("Received hotkey with id: " + hotkeyId);

            _hotkeys[hotkeyId].action();
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
