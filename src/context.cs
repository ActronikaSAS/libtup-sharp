using System;
using System.Runtime.InteropServices;

namespace Tup
{
    public delegate void NewMessageCallback(Context ctx, Message msg);

    public delegate void NativeNewMessageCallback(IntPtr ctx, IntPtr msg,
            IntPtr userdata);

    [StructLayout(LayoutKind.Sequential)]
    public struct Callbacks
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public NativeNewMessageCallback new_msg_cb;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Context : IDisposable
    {
        bool m_disposed = false;
        IntPtr m_ctx;
        GCHandle m_gch;
        NewMessageCallback m_user_cb;

        public Context() {

        }

        public static void callbackNewMessage(Context ctx, Message msg)
        {
            if (msg is Tup.VersionMessage)
            {
                var vmsg = msg as Tup.VersionMessage;
                Console.WriteLine(vmsg.version);
            } else if (msg is Tup.AckMessage)
            {
                // Do nothing
            } else if (msg is Tup.ErrorMessage)
            {
                var emsg = msg as Tup.ErrorMessage;
                Console.WriteLine(emsg.error + " : " + emsg.cmd);
            }
        }

        public void Open(string device)
        {
            Callbacks cbs = new Callbacks();
            m_gch = GCHandle.Alloc(this);

            cbs.new_msg_cb = new NativeNewMessageCallback(Context.onNewMessage);

            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(cbs));
            Marshal.StructureToPtr(cbs, pnt, false);

            m_user_cb = callbackNewMessage;

            m_ctx = create(pnt, GCHandle.ToIntPtr(m_gch));
            if (m_ctx == IntPtr.Zero)
                throw new System.InvalidOperationException();

            int ret = open(device);
            if (ret != 0)
                throw new System.InvalidOperationException();
        }

        public Context(string device, NewMessageCallback cb)
        {
            Callbacks cbs = new Callbacks();
            m_gch = GCHandle.Alloc(this);

            cbs.new_msg_cb = new NativeNewMessageCallback(Context.onNewMessage);

            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(cbs));
            Marshal.StructureToPtr(cbs, pnt, false);

            m_user_cb = cb;

            m_ctx = create(pnt, GCHandle.ToIntPtr(m_gch));
            if (m_ctx == IntPtr.Zero)
                throw new System.InvalidOperationException();

            int ret = open(device);
            if (ret != 0)
                throw new System.InvalidOperationException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Play(int id) {
            send(new Tup.LoadEffectCmdMessage(0, id));
            send(new Tup.BindEffectCmdMessage(0, 3));
            send(new Tup.PlayEffectCmdMessage(0));
        }

        public void SetFilterActive(uint actuatorId, bool active) {
            send(new Tup.SetFilterActive(actuatorId, active));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if (m_ctx != IntPtr.Zero)
                destroy(m_ctx);

            m_gch.Free();

            if (disposing) {
                /* no managable object to dispose */
            }

            m_disposed = true;
        }

        ~Context()
        {
            Dispose(false);
        }

        public int open(string device)
        {
            return open(m_ctx, device);
        }

        public void close()
        {
            close(m_ctx);
        }

        public int send(Message msg)
        {
            return sendNative(m_ctx, msg.toIntPtr());
        }

        public IntPtr getFd()
        {
            return getFdNative(m_ctx);
        }

        public int processFd()
        {
            return processFdNative(m_ctx);
        }

        public int waitAndProcess(int timeout_ms)
        {
            return waitAndProcess(m_ctx, timeout_ms);
        }

        private void callUserCallback(Tup.Message msg)
        {
            m_user_cb(this,msg);
        }

        [DllImport("tup", CharSet=CharSet.Ansi, EntryPoint = "tup_context_new")]
        private static extern IntPtr create(IntPtr cbs, IntPtr userdata);

        [DllImport("tup", EntryPoint = "tup_context_free")]
        private static extern void destroy(IntPtr ctx);


        [DllImport("tup", EntryPoint = "tup_context_open")]
        private static extern int open(IntPtr ctx, string device);

        [DllImport("tup", EntryPoint = "tup_context_close")]
        private static extern void close(IntPtr ctx);

        [DllImport("tup", EntryPoint = "tup_context_get_fd")]
        private static extern IntPtr getFdNative(IntPtr ctx);

        [DllImport("tup", EntryPoint = "tup_context_send")]
        private static extern int sendNative(IntPtr ctx, IntPtr msg);

        [DllImport("tup", EntryPoint = "tup_context_process_fd")]
        private static extern int processFdNative(IntPtr ctx);


        [DllImport("tup", EntryPoint = "tup_context_wait_and_process")]
        private static extern int waitAndProcess(IntPtr ctx, int timeout_ms);

        protected static void onNewMessage(IntPtr ictx, IntPtr imsg,
                IntPtr userdata)
        {
            GCHandle gch = GCHandle.FromIntPtr(userdata);
            Context ctx = (Context)gch.Target;

            switch (NativeMessage.getType(imsg))
            {
                case Tup.MessageType.ACK:
                    ctx.callUserCallback(new AckMessage(imsg));
                    break;
                case Tup.MessageType.ERROR:
                    ctx.callUserCallback(new ErrorMessage(imsg));
                    break;
                case Tup.MessageType.RESP_VERSION:
                    ctx.callUserCallback(new VersionMessage(imsg));
                    break;
            }
        }
    }
}

