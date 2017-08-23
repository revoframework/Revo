namespace GTRevo.Platform.Globalization
{
    public interface ITranslatable
    {
        string Code { get; }
        string Name { get; }
        string Culture { get; set; }
    }
}
