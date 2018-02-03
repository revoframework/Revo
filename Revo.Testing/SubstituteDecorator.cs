using NSubstitute;

namespace Revo.Testing
{
    public static class SubstituteDecorator
    {
        public static T For<T>(T decorated)
            where T : class
        {
            T spy = Substitute.For<T>();

            foreach (var method in typeof(T).GetMethods())
            {
                //spy.WhenForAnyArgs()
            }

            return spy;
        }
    }
}
