using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeepAwake;

public enum PowerRequestType
{

    /// <summary>
    /// This request prevents the computer from automatically turn off the display. 
    /// If the display is already turned off, the the display is turned on.
    /// </summary>
    Display = 0, // not to be used by drivers

    /// <summary>
    /// This request prevents the computer from automatically entering sleep mode after a period of user inactivity.
    /// This request type is not honored on systems capable of connected standby. Applications should use <see cref="PowerRequestType.Execution"/> instead.
    /// </summary>
    System = 1,

    /// <summary>
    /// The system enters away mode instead of sleep in response to explicit action by the user. 
    /// In away mode, the system continues to run but turns off audio and video to give the appearance of sleep.
    /// </summary>
    AwayMode = 2, // not to be used by drivers   

    /// <summary>
    /// The calling process continues to run instead of being suspended or terminated by process lifetime management mechanisms.
    /// When and how long the process is allowed to run depends on the operating system and power policy settings.
    /// On systems not capable of connected standby, <see cref="PowerRequestType.Execution"/> is equivalent to <see cref="PowerRequestType.System"/>.
    /// </summary>
    /// <remarks>
    /// PowerRequestType.Execution is supported starting with Windows 8 and Windows Server 2012.
    /// </remarks>
    Execution = 3, // not to be used by drivers
}

public class PowerRequest : IDisposable
{

    private readonly PowerRequestType powerRequestType;
    private readonly IntPtr powerRequestHandle;

    public PowerRequest(PowerRequestType powerRequestType, IntPtr powerRequestHandle)
    {
        this.powerRequestType = powerRequestType;
        this.powerRequestHandle = powerRequestHandle;
    }

    public void Dispose()
    {
        if (!PowerClearRequest(powerRequestHandle, powerRequestType))
        {
            throw new Exception("Could not clear power request!");
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool PowerClearRequest(IntPtr powerRequestHandle, PowerRequestType powerRequestType);

}

public static class PowerManagement
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PowerRequestContext
    {
        public UInt32 Version;
        public UInt32 Flags;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string SimpleReasonString;
    }

    private const int PowerRequestContextVersion = 0;
    private const int PowerRequestContextSimpleString = 0x1;

    public static PowerRequest CreatePowerRequest(PowerRequestType powerRequestType, string reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            throw new ArgumentException(nameof(reason));
        }
        PowerRequestContext powerRequestContext = new PowerRequestContext
        {
            Flags = PowerRequestContextSimpleString,
            Version = PowerRequestContextVersion,
            SimpleReasonString = reason
        };
        IntPtr powerRequestHandle = PowerCreateRequest(ref powerRequestContext);
        if (powerRequestHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        if (!PowerSetRequest(powerRequestHandle, powerRequestType))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        return new PowerRequest(powerRequestType, powerRequestHandle);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr PowerCreateRequest(ref PowerRequestContext context);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool PowerSetRequest(IntPtr powerRequestHandle, PowerRequestType powerRequestType);

}