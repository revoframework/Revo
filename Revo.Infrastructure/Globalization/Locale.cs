using System.Globalization;

namespace Revo.Infrastructure.Globalization
{
    public class Locale
    {
        public Locale(string code)
        {
            Code = code;
            CultureInfo = CultureInfo.GetCultureInfo(code);
        }

        public string Code { get; private set; }
        public CultureInfo CultureInfo { get; private set; }
    }
}
