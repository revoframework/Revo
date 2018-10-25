using System;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;
using Knoema.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Revo.Core.Globalization;

namespace Revo.AspNet.Globalization
{
    public class TranslatingJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);
            var code = (string)t[nameof(ITranslatable.Code)];
            var culture = (string)t[nameof(ITranslatable.Culture)];
            var translatedTypeName =
                value.GetType().GetAttributes<TranslateAsAttribute>().FirstOrDefault()?.Type.FullName ??
                value.GetType().FullName;
            t[nameof(ITranslatable.Name)] = Translate($"{translatedTypeName}.{code}", culture ?? "cs-CZ");
            t.WriteTo(writer);
        }

        private string Translate(string key, string culture)
        {
            var translation = LocalizationManager.Provider.Get(CultureInfo.CreateSpecificCulture(culture), null, key);
            return $"{translation.Translation ?? key}";
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ITranslatable).IsAssignableFrom(objectType);
        }

        public override bool CanRead => false;
        public override bool CanWrite => true;
    }
}