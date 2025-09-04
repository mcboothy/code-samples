namespace GlobalMouseEvents;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static GlobalMouseEvents.NativeHelper;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private HwndSource? _source;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _source = (HwndSource)PresentationSource.FromVisual(this);
        _source.AddHook(WndProc);

        RAWINPUTDEVICE input = new() 
        {
            usUsagePage = 0x01,
            usUsage = 0x02, // Mouse
            dwFlags = RIDEV_INPUTSINK,
            hwndTarget = _source.Handle
        };

        if (!RegisterRawInputDevices(ref input, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
        {
            MessageBox.Show($"Stuff went wrong RegisterRawInputDevices: Error Code : {Marshal.GetLastWin32Error()}");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_INPUT:
                {
                    // call GetRawInputData to get the size of the data RAWINPUTHEADER
                    uint dwSize = 0;
                    GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf<RAWINPUTHEADER>());
                    IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
                    try
                    {
                        // call it again to return the buffer
                        if (GetRawInputData(lParam, RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf<RAWINPUTHEADER>()) == dwSize)
                        {
                            RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);
                            if (raw.header.dwType == RM_TYPEMOUSE)
                            {
                                OnGlobalMouseMove(raw.mouse.lLastX, raw.mouse.lLastY);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    handled = false;
                }
                break;
        }

        return IntPtr.Zero;
    }

    private void OnGlobalMouseMove(int dx, int dy)
    {
        Debug.WriteLine($"Mouse Moved : x = {dx}, y = {dy}");
    }
}