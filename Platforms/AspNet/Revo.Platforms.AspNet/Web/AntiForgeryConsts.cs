namespace Revo.Platforms.AspNet.Web
{
    public static class AntiForgeryConsts
    {
        public const string CookieTokenName = "revoCsrfToken";
        public const string CookieFormTokenName = "revoCsrfFormToken";
        public const string HeaderTokenName = "Revo-Csrf-Token";
    }
}
