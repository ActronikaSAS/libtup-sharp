using System;
using System.Runtime.InteropServices;

namespace Tup
{
    abstract public class Message
    {
        protected IntPtr m_msg;

        public Message()
        {
            m_msg = NativeMessage.create();
        }

        ~Message()
        {
            NativeMessage.destroy(m_msg);
        }

        public IntPtr toIntPtr()
        {
            return m_msg;
        }
    }

    /* Command messages */
    public class GetVersionCmdMessage : Message
    {
        public GetVersionCmdMessage()
        {
            NativeMessage.initGetVersion(m_msg);
        }
    }

    public class LoadEffectCmdMessage : Message
    {
        public LoadEffectCmdMessage(int effect_id, int bank_id)
        {
            NativeMessage.initLoad(m_msg, effect_id, bank_id);
        }
    }

    public class PlayEffectCmdMessage : Message
    {
        public PlayEffectCmdMessage(int effect_id)
        {
            NativeMessage.initPlay(m_msg, effect_id);
        }
    }

    public class StopEffectCmdMessage : Message
    {
        public StopEffectCmdMessage(int effect_id)
        {
            NativeMessage.initStop(m_msg, effect_id);
        }
    }

    public class BindEffectCmdMessage : Message
    {
        public BindEffectCmdMessage(int effect_id, uint binding_flags)
        {
            NativeMessage.initBindEffect(m_msg, effect_id, binding_flags);
        }
    }

    public class SetParameterCmdMessage : Message
    {
        public SetParameterCmdMessage(uint effect_id, uint parameter_id,
                uint parameter_value)
        {
            NativeMessage.initSetParameter(m_msg, effect_id, parameter_id,
                    parameter_value);
        }

        public SetParameterCmdMessage(uint effect_id, ParameterArgs[] args)
        {
            NativeMessage.initSetParameter(m_msg, effect_id, args,
                    (uint) args.Length);
        }
    }

    public class SetInputCmdMessage : Message
    {
        public SetInputCmdMessage(uint effect_id, uint input_id,
                int input_value)
        {
            NativeMessage.initSetInput(m_msg, effect_id, input_id, input_value);
        }

        public SetInputCmdMessage(uint effect_id, InputArgs[] args)
        {
            NativeMessage.initSetInput(m_msg, effect_id, args,
                    (uint) args.Length);
        }
    }

    /* Response message */
    public class AckMessage : Message
    {
        public MessageType cmd;

        public AckMessage(IntPtr imsg)
        {
            NativeMessage.parseAck(imsg, out cmd);
        }
    }

    public class ErrorMessage : Message
    {
        public MessageType cmd;
        public uint error;


        public ErrorMessage(IntPtr imsg)
        {
            NativeMessage.parseError(imsg, out cmd, out error);
        }
    }

    public class VersionMessage : Message
    {
        public string version;

        public VersionMessage(IntPtr imsg)
        {
            IntPtr ver;

            NativeMessage.parseRespVersion(imsg, out ver);
            version = Marshal.PtrToStringAnsi(ver);
        }
    }

    public class SetIVModelDeviceTypeMessage : Message
    {
        public SetIVModelDeviceTypeMessage(uint type)
        {
            NativeMessage.initSetIvModelDeviceType(m_msg, type);
        }
    }

    public class SetIVModelActive : Message
    {
        public SetIVModelActive(bool active)
        {
            NativeMessage.initSetIvModelActive(m_msg, (uint) (active ? 1 : 0));
        }
    }
}
