namespace GTRevo.Platform.IO.Messages
{
    public interface IMessageSource
    {
        //IReadOnlyDictionary<string, string> Messages { get; }
        bool TryGetMessage(string key, out string message);
    }
}
