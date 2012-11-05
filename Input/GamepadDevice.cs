using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Blueberry.XInput;

namespace Blueberry.Input
{
    public delegate void GamepadButtonHandler(object sender, GamepadButtonFlags e);
    public delegate void GamepadStickHandler(object sender, GamepadDevice.ThumbstickState e, Vector2 delta);
    public delegate void GamepadTriggerHandler(object sender, GamepadDevice.TriggerState e);

    public class GamepadDevice
    {
        public event GamepadButtonHandler OnButtonDown;
        public event GamepadButtonHandler OnButtonUp;
        public event GamepadButtonHandler OnButtonPress;

        public event GamepadTriggerHandler OnLeftTrigger;
        public event GamepadTriggerHandler OnRightTrigger;

        public event GamepadStickHandler OnLeftStick;
        public event GamepadStickHandler OnRightStick;

        int lastPacket;

        public GamepadDevice(UserIndex userIndex)
        {
            UserIndex = userIndex;
            Controller = new Controller((UserIndex)userIndex);
        }

        public readonly UserIndex UserIndex;

        internal readonly Controller Controller;

        private Gamepad old_state;

        public DPadState DPad { get; private set; }

        public ThumbstickState LeftStick { get; private set; }

        public ThumbstickState RightStick { get; private set; }

        public bool A { get; private set; }

        public bool B { get; private set; }

        public bool X { get; private set; }

        public bool Y { get; private set; }

        public bool RightShoulder { get; private set; }

        public bool LeftShoulder { get; private set; }

        public bool Start { get; private set; }

        public bool Back { get; private set; }

        public float RightTrigger { get; private set; }

        public float LeftTrigger { get; private set; }

        private bool _conected;

        public bool Connected
        {
            get { return _conected; }
        }

        private static float Saturate(float value)
        {
            return value < 0 ? 0 : value > 1 ? 1 : value;
        }

        private float _elapsed;
        private float _vibrationTime;


        public void Vibrate(float leftMotor, float rightMotor)
        {
            if (!Connected) return;
            Controller.SetVibration(new Vibration
            {
                LeftMotorSpeed = (ushort)(Saturate(leftMotor) * ushort.MaxValue),
                RightMotorSpeed = (ushort)(Saturate(rightMotor) * ushort.MaxValue)
            });
        }

        public void Vibrate(float leftMotor, float rightMotor, float time)
        {
            Vibrate(leftMotor, rightMotor);
            _elapsed = 0;
            _vibrationTime = time;
        }

        public void Update(float dt)
        {
            ThumbstickState old_l = LeftStick;
            ThumbstickState old_r = RightStick;

            _conected = Controller.IsConnected;
            // If not connected, nothing to update
            if (!_conected) return;

            if (_vibrationTime > _elapsed)
            {
                _elapsed += dt;
            }
            else
            {
                _elapsed = 0;
                _vibrationTime = 0;
                Vibrate(0, 0);
            }

            // If same packet, nothing to update
            State state = Controller.GetState();
            if (lastPacket == state.PacketNumber) return;
            lastPacket = state.PacketNumber;

            var gamepadState = state.Gamepad;

            // Shoulders
            LeftShoulder = (gamepadState.Buttons & GamepadButtonFlags.LeftShoulder) != 0;
            RightShoulder = (gamepadState.Buttons & GamepadButtonFlags.RightShoulder) != 0;

            // Triggers
            LeftTrigger = gamepadState.LeftTrigger / (float)byte.MaxValue;
            RightTrigger = gamepadState.RightTrigger / (float)byte.MaxValue;

            // Buttons
            Start = (gamepadState.Buttons & GamepadButtonFlags.Start) != 0;
            Back = (gamepadState.Buttons & GamepadButtonFlags.Back) != 0;

            A = (gamepadState.Buttons & GamepadButtonFlags.A) != 0;
            B = (gamepadState.Buttons & GamepadButtonFlags.B) != 0;
            X = (gamepadState.Buttons & GamepadButtonFlags.X) != 0;
            Y = (gamepadState.Buttons & GamepadButtonFlags.Y) != 0;

            // D-Pad
            DPad = new DPadState((gamepadState.Buttons & GamepadButtonFlags.DPadUp) != 0,
                                 (gamepadState.Buttons & GamepadButtonFlags.DPadDown) != 0,
                                 (gamepadState.Buttons & GamepadButtonFlags.DPadLeft) != 0,
                                 (gamepadState.Buttons & GamepadButtonFlags.DPadRight) != 0);

            // Thumbsticks
            LeftStick = new ThumbstickState(
                Normalize(gamepadState.LeftThumbX, gamepadState.LeftThumbY, Gamepad.LeftThumbDeadZone),
                (gamepadState.Buttons & GamepadButtonFlags.LeftThumb) != 0);
            RightStick = new ThumbstickState(
                Normalize(gamepadState.RightThumbX, gamepadState.RightThumbY, Gamepad.RightThumbDeadZone),
                (gamepadState.Buttons & GamepadButtonFlags.RightThumb) != 0);

            foreach (var flag in Enum.GetValues(typeof(GamepadButtonFlags)))
            {
                if (OnButtonUp != null)
                    if ((old_state.Buttons & (GamepadButtonFlags)flag) != 0 && (gamepadState.Buttons & (GamepadButtonFlags)flag) == 0)
                        OnButtonUp(this, (GamepadButtonFlags)((GamepadButtonFlags)flag));
                if (OnButtonPress != null)
                {
                    if ((old_state.Buttons & (GamepadButtonFlags)flag) == 0 && (gamepadState.Buttons & (GamepadButtonFlags)flag) != 0)
                        OnButtonPress(this, (GamepadButtonFlags)((GamepadButtonFlags)flag));
                }
                if (OnButtonDown != null)
                    if ((gamepadState.Buttons & (GamepadButtonFlags)flag) != 0)
                        OnButtonDown(this, (GamepadButtonFlags)gamepadState.Buttons);
            }

            if (OnLeftTrigger != null)
                if (gamepadState.LeftTrigger != old_state.LeftTrigger)
                    OnLeftTrigger(this, new TriggerState(LeftTrigger, LeftTrigger - (old_state.LeftTrigger / (float)byte.MaxValue)));
            if (OnRightTrigger != null)
                if (gamepadState.RightTrigger != old_state.RightTrigger)
                    OnRightTrigger(this, new TriggerState(RightTrigger, RightTrigger - (old_state.RightTrigger / (float)byte.MaxValue)));

            if (OnLeftStick != null)
                if (LeftStick.Position != old_l.Position || LeftStick.Clicked != old_l.Clicked)
                    OnLeftStick(this, LeftStick, LeftStick.Position - old_l.Position);
            if (OnRightStick != null)
                if (RightStick.Position != old_r.Position || RightStick.Clicked != old_r.Clicked)
                    OnRightStick(this, RightStick, RightStick.Position - old_r.Position);



            old_state = gamepadState;
        }

        private static Vector2 Normalize(short rawX, short rawY, short threshold)
        {
            var value = new Vector2(rawX, -rawY);
            var magnitude = value.Length;
            var direction = value / (magnitude == 0 ? 1 : magnitude);

            var normalizedMagnitude = 0.0f;
            if (magnitude - threshold > 0)
                normalizedMagnitude = Math.Min((magnitude - threshold) / (short.MaxValue - threshold), 1);

            return direction * normalizedMagnitude;
        }

        public struct TriggerState
        {
            public readonly float value, delta;

            public TriggerState(float value, float delta)
            {
                this.value = value;
                this.delta = delta;
            }
        }
        public struct ButtonsState
        {
            public readonly bool A, B, X, Y, LeftShoulder, RightShoulder, LeftThumb, RightThumb, Start, Back;

            public ButtonsState(bool A, bool B, bool X, bool Y, bool LeftShoulder, bool RightShoulder, bool LeftThumb, bool RightThumb, bool Start, bool Back)
            {
                this.A = A;
                this.B = B;
                this.X = X;
                this.Y = Y;
                this.LeftShoulder = LeftShoulder;
                this.RightShoulder = RightShoulder;
                this.LeftThumb = LeftThumb;
                this.RightThumb = RightThumb;
                this.Start = Start;
                this.Back = Back;
            }
        }

        public struct DPadState
        {
            public readonly bool Up, Down, Left, Right;

            public DPadState(bool up, bool down, bool left, bool right)
            {
                Up = up; Down = down; Left = left; Right = right;
            }
        }

        public struct ThumbstickState
        {
            public readonly Vector2 Position;
            public readonly bool Clicked;

            public ThumbstickState(Vector2 position, bool clicked)
            {
                Clicked = clicked;
                Position = position;
            }
        }
    }
}