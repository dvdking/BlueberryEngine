#if WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Blueberry.XInput
{
    /// <summary>
    /// Functions
    /// </summary>
    internal static class XInput
    {
        /// <summary>
        /// Retrieves a gamepad input event.
        /// </summary>
        public static unsafe int XInputGetKeystroke(int dwUserIndex, int dwReserved, out Keystroke keystrokeRef)
        {
            keystrokeRef = new Keystroke();
            int keystroke;
            fixed (Keystroke* keystrokePtr = &keystrokeRef)
                keystroke = XInput.XInputGetKeystroke_(dwUserIndex, dwReserved, (void*)keystrokePtr);
            return keystroke;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetKeystroke", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern int XInputGetKeystroke_(int arg0, int arg1, void* arg2);

        /// <summary>
        /// Sends data to a connected controller. This function is used to activate the vibration function of a controller.
        /// </summary>
        public static unsafe int XInputSetState(int dwUserIndex, Vibration vibrationRef)
        {
            return XInput.XInputSetState_(dwUserIndex, (void*)&vibrationRef);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("xinput1_3.dll", EntryPoint = "XInputSetState", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern int XInputSetState_(int arg0, void* arg1);
    
        /// <summary>
        /// Gets the sound rendering and sound capture device GUIDs that are associated with the headset connected to the specified controller.
        /// 
        /// </summary>
        public static unsafe int XInputGetDSoundAudioDeviceGuids(int dwUserIndex, out Guid dSoundRenderGuidRef, out Guid dSoundCaptureGuidRef)
        {
            dSoundRenderGuidRef = new Guid();
            dSoundCaptureGuidRef = new Guid();
            int audioDeviceGuids;
            fixed (Guid* guidPtr1 = &dSoundRenderGuidRef)
            fixed (Guid* guidPtr2 = &dSoundCaptureGuidRef)
                audioDeviceGuids = XInput.XInputGetDSoundAudioDeviceGuids_(dwUserIndex, (void*)guidPtr1, (void*)guidPtr2);
            return audioDeviceGuids;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetDSoundAudioDeviceGuids", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern int XInputGetDSoundAudioDeviceGuids_(int arg0, void* arg1, void* arg2);

        /// <summary>
        /// Retrieves the current state of the specified controller.
        /// </summary>
        public static unsafe int XInputGetState(int dwUserIndex, out Blueberry.XInput.State stateRef)
        {
            stateRef = new State();
            int state;
            fixed (State* statePtr = &stateRef)
                state = XInput.XInputGetState_(dwUserIndex, (void*)statePtr);
            return state;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern int XInputGetState_(int arg0, void* arg1);

        /// <summary>
        /// Retrieves the capabilities and features of a connected controller.
        /// </summary>
        public static unsafe int XInputGetCapabilities(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities capabilitiesRef)
        {
            capabilitiesRef = new Capabilities();
            int capabilities;
            fixed (Capabilities* capabilitiesPtr = &capabilitiesRef)
                capabilities = XInput.XInputGetCapabilities_(dwUserIndex, (int)dwFlags, (void*)capabilitiesPtr);
            return capabilities;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetCapabilities", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern int XInputGetCapabilities_(int arg0, int arg1, void* arg2);

        /// <summary>
        /// Retrieves the battery type and charge status of a wireless controller.
        /// </summary>
        public static unsafe int XInputGetBatteryInformation(int dwUserIndex, BatteryDeviceType devType, out BatteryInformation batteryInformationRef)
        {
            batteryInformationRef = new BatteryInformation();
            int batteryInformation;
            fixed (BatteryInformation* batteryInformationPtr = &batteryInformationRef)
                batteryInformation = XInput.XInputGetBatteryInformation_(dwUserIndex, (int)devType, (void*)batteryInformationPtr);
            return batteryInformation;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetBatteryInformation", CallingConvention = CallingConvention.StdCall)]
        private static unsafe extern int XInputGetBatteryInformation_(int arg0, int arg1, void* arg2);

        /// <summary>
        /// Sets the reporting state of XInput.
        /// </summary>
        public static unsafe void XInputEnable(Bool enable)
        {
            XInput.XInputEnable_(enable);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("xinput1_3.dll", EntryPoint = "XInputEnable", CallingConvention = CallingConvention.StdCall)]
        private static extern void XInputEnable_(Bool arg0);
    }
}
#endif