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

        private Context() {}

        public Context(string device, NewMessageCallback cb)
        {
            Callbacks cbs = new Callbacks();
            m_gch = GCHandle.Alloc(this);

            cbs.new_msg_cb = new NativeNewMessageCallback(Context.onNewMessage);

            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(cbs));
            Marshal.StructureToPtr(cbs, pnt, false);

            m_user_cb = cb;

            m_ctx = create(device, pnt, GCHandle.ToIntPtr(m_gch));
            if (m_ctx == IntPtr.Zero)
                throw new System.InvalidOperationException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private void callUserCallback(Tup.Message msg)
        {
            m_user_cb(this,msg);
        }

        [DllImport("tup", CharSet=CharSet.Ansi, EntryPoint = "tup_context_new")]
        private static extern IntPtr create(string device, IntPtr cbs,
                IntPtr userdata);

        [DllImport("tup", EntryPoint = "tup_context_free")]
        private static extern void destroy(IntPtr ctx);

        [DllImport("tup", EntryPoint = "tup_context_get_fd")]
        private static extern IntPtr getFdNative(IntPtr ctx);

        [DllImport("tup", EntryPoint = "tup_context_send")]
        private static extern int sendNative(IntPtr ctx, IntPtr msg);

        [DllImport("tup", EntryPoint = "tup_context_process_fd")]
        private static extern int processFdNative(IntPtr ctx);

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

