using System;
using System.Runtime.InteropServices;

namespace Tup
{
    public enum MessageType {
        ACK = 1,
        ERROR = 2,
        CMD_LOAD = 10,
        CMD_PLAY = 11,
        CMD_STOP = 12,
        CMD_GET_VERSION = 13,
        CMD_GET_PARAMETER = 14,
        CMD_SET_PARAMETER = 15,
        CMD_BIND_EFFECT = 16,
        CMD_GET_SENSOR_VALUE = 17,
        CMD_SET_SENSOR_VALUE = 18,
        CMD_GET_BUILDINFO = 19,
        CMD_ACTIVATE_INTERNAL_SENSORS = 20,
        CMD_GET_INPUT_VALUE = 21,
        CMD_SET_INPUT_VALUE = 22,
        CMD_SET_IV_MODEL_GET_ACTIVE = 23,     // NOT IMPLEMENTED
        CMD_SET_IV_MODEL_SET_ACTIVE = 24,
        CMD_CONFIG_WRITE = 25,                // NOT IMPLEMENTED
        CMD_CONFIG_BAND_NORM_GET_COEFFS = 26, // NOT IMPLEMENTED
        CMD_CONFIG_BAND_NORM_SET_COEFFS = 27, // NOT IMPLEMENTED


        RESP_VERSION = 100,
        RESP_PARAMETER = 101,
        RESP_SENSOR = 102,
        RESP_BUILDINFO = 103,
        RESP_INPUT = 104,
        RESP_SET_PARAMETER = 105,
        RESP_FILTER_ACTIVE = 106,
        RESP_BAND_NORM_COEFFS = 107,       // NOT IMPLEMENTED

        CMD_DEBUG_GET_SYSTEM_STATUS = 200, // NOT IMPLEMENTED
        RESP_DEBUG_SYSTEM_STATUS = 201     // NOT IMPLEMENTED
    };

    public enum FilterId
    {
        FILTER_ID_NONE = 0,
        FILTER_ID_BAND_NORM = 1
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ParameterArgs
    {
        byte parameter_id;

        [MarshalAs (UnmanagedType.U4)]
        uint parameter_value;

        public ParameterArgs(byte id, uint val)
        {
            parameter_id = id;
            parameter_value = val;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InputArgs
    {
        byte input_id;

        [MarshalAs (UnmanagedType.U4)]
        int input_value;

        public InputArgs(byte id, int val)
        {
            input_id = id;
            input_value = val;
        }
    }

    public class NativeMessage
    {
        [DllImport("tup", EntryPoint = "tup_message_new")]
        public static extern IntPtr create();

        [DllImport("tup", EntryPoint = "tup_message_free")]
        public static extern void destroy(IntPtr msg);

        [DllImport("tup", EntryPoint = "tup_message_get_type")]
        public static extern MessageType getType(IntPtr msg);

        [DllImport("tup", EntryPoint = "tup_message_parse_ack")]
        public static extern int parseAck(IntPtr msg, out MessageType type);

        [DllImport("tup", EntryPoint = "tup_message_parse_error")]
        public static extern int parseError(IntPtr msg, out MessageType type,
                out uint error);

        [DllImport("tup", EntryPoint = "tup_message_init_load")]
        public static extern void initLoad(IntPtr msg, int effect_id,
                int bank_id);

        [DllImport("tup", EntryPoint = "tup_message_init_play")]
        public static extern void initPlay(IntPtr msg, int effect_id);

        [DllImport("tup", EntryPoint = "tup_message_init_stop")]
        public static extern void initStop(IntPtr msg, int effect_id);

        [DllImport("tup", EntryPoint = "tup_message_init_get_version")]
        public static extern void initGetVersion(IntPtr msg);

        [DllImport("tup", EntryPoint = "tup_message_init_bind_effect")]
        public static extern void initBindEffect(IntPtr msg, int effect_id,
                uint binding_flags);

        [DllImport("tup", EntryPoint = "tup_message_init_set_parameter_simple")]
        public static extern void initSetParameter(IntPtr msg, uint effect_id,
                uint parameter_id, uint parameter_value);

        [DllImport("tup", EntryPoint = "tup_message_init_set_parameter_array")]
        public static extern int initSetParameter(IntPtr msg, uint effect_id,
                ParameterArgs[] args, uint size);

        [DllImport("tup", EntryPoint = "tup_message_init_set_input_value_simple")]
        public static extern void initSetInput(IntPtr msg, uint effect_id,
                uint input_id, int input_value);

        [DllImport("tup", EntryPoint = "tup_message_init_set_input_value_array")]
        public static extern int initSetInput(IntPtr msg, uint effect_id,
                InputArgs[] args, uint size);

        [DllImport("tup", CharSet = CharSet.Ansi, EntryPoint = "tup_message_parse_resp_version")]
        public static extern int parseRespVersion(IntPtr msg, out IntPtr version);

        [DllImport("tup", CharSet = CharSet.Ansi, EntryPoint = "tup_message_init_filter_set_active")]
        public static extern int initSetFilterActive(IntPtr msg, FilterId filter, uint actuator_id, bool active);

    }
}
