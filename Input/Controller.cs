
using System;
using Blueberry.Input;

namespace Blueberry.XInput
{
    /// <summary>
    /// A XInput controller.
    /// </summary>
    internal class Controller
    {
        private readonly UserIndex userIndex;

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// 
        /// </value>
        public bool IsConnected
        {
            get
            {
#if WINDOWS
                Blueberry.XInput.State stateRef;
                return XInput.XInputGetState((int)this.userIndex, out stateRef) == 0;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets the sound render GUID.
        /// 
        /// </summary>
        public Guid SoundRenderGuid
        {
            get
            {
#if WINDOWS
                Guid dSoundRenderGuidRef;
                Guid dSoundCaptureGuidRef;
                XInput.XInputGetDSoundAudioDeviceGuids((int)this.userIndex, out dSoundRenderGuidRef, out dSoundCaptureGuidRef);
                return dSoundRenderGuidRef;
#else
                return Guid.Empty;
#endif
            }
        }

        /// <summary>
        /// Gets the sound capture GUID.
        /// 
        /// </summary>
        public Guid SoundCaptureGuid
        {
            get
            {
#if WINDOWS
                Guid dSoundRenderGuidRef;
                Guid dSoundCaptureGuidRef;
                XInput.XInputGetDSoundAudioDeviceGuids((int)this.userIndex, out dSoundRenderGuidRef, out dSoundCaptureGuidRef);
                return dSoundCaptureGuidRef;
#else
                return Guid.Empty;
#endif
            }
        }

        /// <summary>
        /// Initializes a new instance of the Controller class.
        /// 
        /// </summary>
        /// <param name="userIndex">Index of the user.</param>
        public Controller(UserIndex userIndex = UserIndex.Any)
        {
            this.userIndex = userIndex;
        }

        /// <summary>
        /// Gets the battery information.
        /// 
        /// </summary>
        /// <param name="batteryDeviceType">Type of the battery device.</param>
        /// <returns/>
        /// <unmanaged>unsigned int XInputGetBatteryInformation([In] XUSER_INDEX dwUserIndex,[In] BATTERY_DEVTYPE devType,[Out] XINPUT_BATTERY_INFORMATION* pBatteryInformation)</unmanaged>
        public BatteryInformation GetBatteryInformation(BatteryDeviceType batteryDeviceType)
        {
#if WINDOWS
            BatteryInformation batteryInformationRef;
            ErrorCodeHelper.ToResult(XInput.XInputGetBatteryInformation((int)this.userIndex, batteryDeviceType, out batteryInformationRef)).CheckError();
            return batteryInformationRef;
#else
            return BatteryInformation.Empty;
#endif
        }

        /// <summary>
        /// Gets the capabilities.
        /// 
        /// </summary>
        /// <param name="deviceQueryType">Type of the device query.</param>
        /// <returns/>
        public Capabilities GetCapabilities(DeviceQueryType deviceQueryType)
        {
#if WINDOWS
            Capabilities capabilitiesRef;
            ErrorCodeHelper.ToResult(XInput.XInputGetCapabilities((int)this.userIndex, deviceQueryType, out capabilitiesRef)).CheckError();
            return capabilitiesRef;
#else
            return Capabilities.Empty;
#endif
        }

        /// <summary>
        /// Gets the keystroke.
        /// 
        /// </summary>
        /// <param name="deviceQueryType">The flag.</param><param name="keystroke">The keystroke.</param>
        /// <returns/>
        public Result GetKeystroke(DeviceQueryType deviceQueryType, out Keystroke keystroke)
        {
#if WINDOWS
            Result result = ErrorCodeHelper.ToResult(XInput.XInputGetKeystroke((int)this.userIndex, (int)deviceQueryType, out keystroke));
            result.CheckError();
            return result;
#else
            keystroke = new Keystroke();
            return Result.Abord;
#endif
        }

        /// <summary>
        /// Gets the state.
        /// 
        /// </summary>
        /// 
        /// <returns/>
        public State GetState()
        {
#if WINDOWS
            Blueberry.XInput.State stateRef;
            ErrorCodeHelper.ToResult(XInput.XInputGetState((int)this.userIndex, out stateRef)).CheckError();
            return stateRef;
#else
            return new State();
#endif
        }

        /// <summary>
        /// Sets the reporting.
        /// </summary>
        /// <param name="enableReporting">if set to <c>true</c> [enable reporting].</param>
        public static void SetReporting(bool enableReporting)
        {
#if WINDOWS
            XInput.XInputEnable((Bool)enableReporting);
#else
            
#endif
        }

        /// <summary>
        /// Sets the vibration.
        /// 
        /// </summary>
        /// <param name="vibration">The vibration.</param>
        /// <returns/>
        public Result SetVibration(Vibration vibration)
        {
#if WINDOWS
            Result result = ErrorCodeHelper.ToResult(XInput.XInputSetState((int)this.userIndex, vibration));
            result.CheckError();
            return result;
#else
            return Result.Abord;
#endif
        }
    }
}
