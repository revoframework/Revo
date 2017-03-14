using System;

namespace GTRevo.Platform.Web.VirtualPath
{
    public static class Logger
    {
        static Logger()
        {
            LogWarning = (message, ex) => { };
        }

        public static Action<string, Exception> LogWarning { get; set; }
    }
}
