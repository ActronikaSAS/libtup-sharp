using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Tup;

namespace MyApp
{
    class Program
    {
        public static void onNewMessage(Tup.Context ctx, Tup.Message msg)
        {
            if (msg is Tup.VersionMessage) {
                var vmsg = msg as Tup.VersionMessage;

                Console.WriteLine("Version: " + vmsg.version);
            } else if (msg is Tup.AckMessage) {
                /* do nothing */
            } else if (msg is Tup.ErrorMessage) {
                var emsg = msg as Tup.ErrorMessage;

                Console.WriteLine("Error " + emsg.error + " while sending "
                        + emsg.cmd);
            } else {
                Console.WriteLine("unhandled message");
            }
        }

        static void Main()
        {
            Tup.Context ctx = new Tup.Context("/dev/ttyUSB0", onNewMessage);
            if (ctx == null)
                return;

            Console.WriteLine("context initialized");

            while (true) {
                var msg = new Tup.GetVersionCmdMessage();
                ctx.send(msg);
                ctx.processFd();

                var msg1 = new Tup.LoadEffectCmdMessage(0, 4);
                ctx.send(msg1);

                var msg2 = new Tup.BindEffectCmdMessage(0, 3);
                ctx.send(msg2);

                var msg3 = new Tup.PlayEffectCmdMessage(0);
                ctx.send(msg3);

                var msg4 = new Tup.SetParameterCmdMessage(0, 3, 42);
                ctx.send(msg4);

                Tup.ParameterArgs[] args = new Tup.ParameterArgs[] {
                    new ParameterArgs(0, 50),
                    new ParameterArgs(1, 51),
                };
                var msg5 = new Tup.SetParameterCmdMessage(0, args);
                ctx.send(msg5);

                Thread.Sleep(1 * 1000);
            }
        }
    }
}
