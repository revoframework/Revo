using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Web
{
    public static class AntiForgeryConsts
    {
        public const string CookieTokenName = "revoCsrfToken";
        public const string CookieFormTokenName = "revoCsrfFormToken";
        public const string HeaderTokenName = "Revo-Csrf-Token";
    }
}
