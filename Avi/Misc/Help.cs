using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using static Inputs.Misc.Native.Kernel32;

namespace Inputs.Misc {
    internal static class Help {
        public static Point<int> CalculateAbsolutePosition(int x, int y) {
            var origin = Mouse.GetCursorPos();

            Screen screen = Screen.FromPoint(new Point() {
                X = origin.X,
                Y = origin.Y
            });

            // as we are using the MOUSEEVENTF_ABSOLUTE-flag, we must get the absolute position of the original position
            // we then add the new X and Y (absolute) to the origin to move the mouse relative to its location
            int absoluteOriginX = ((int)(65536.0 / screen.Bounds.Width * origin.X));
            int absoluteOriginY = ((int)(65536.0 / screen.Bounds.Height * origin.Y));

            int newX = absoluteOriginX + ((int)(65536.0 / screen.Bounds.Width * x)) + 1;
            int newY = absoluteOriginY + ((int)(65536.0 / screen.Bounds.Height * y)) + 1;

            return new Point<int>(newX, newY);
        }

        public static void DispatchInThread(Action block) {
            if (block != null) {
                new Thread(() => block()) {
                    IsBackground = true
                }.Start();
            }
        }

        public static byte[] GetUnmanagedFunctionBytes(string library, string name, int bufSize = 128) {
            try {
                if (string.IsNullOrWhiteSpace(library) || string.IsNullOrWhiteSpace(name))
                    return [];

                IntPtr handle = GetModuleHandleW(library);
                IntPtr address = GetProcAddress(handle, name);

                if (address == IntPtr.Zero)
                    return [];

                byte[] buffer = new byte[bufSize];
                Marshal.Copy(address, buffer, 0, bufSize);
                return buffer;
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            return [];
        }


        public static T FromBinaryReader<T>(BinaryReader reader) {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            // Alloc the bytes
            IntPtr addy = VirtualAlloc(IntPtr.Zero, (uint)bytes.Length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            // Copy the bytes to the address
            Marshal.Copy(bytes, 0, addy, bytes.Length);

            T theStructure = (T)Marshal.PtrToStructure(addy, typeof(T))!;

            VirtualFree(addy, bytes.Length, FreeType.Release);

            /*
            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            T theStructure;

            try
            {
                theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free(); 
            }
            */

            return theStructure;
        }

        public static unsafe T ByteArrayToStructure<T>(byte[] bytes) where T : struct {
            fixed (byte* ptr = &bytes[0]) {
                return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T))!;
            }
        }

        public static byte[] ToBytes(this object str) {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static bool ExtractResourceTo(string resource, string path) {
            try {
                using Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)!;
                if (File.Exists(path))
                    File.Delete(path);

                using FileStream s = File.OpenWrite(path);
                input.CopyTo(s);

                return true;
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool SendCommand(string command, string args) {
            try {
                ProcessStartInfo si = new() {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                Process temp = new() {
                    StartInfo = si,
                    EnableRaisingEvents = true
                };
                temp.Start();

#if DEBUG
                Console.WriteLine(temp.StandardOutput.ReadToEnd());
#endif          

                temp.WaitForExit(3000);

                return true;
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            return false;
        }
    }
}
