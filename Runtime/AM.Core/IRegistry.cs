namespace AM.Core
{
    public interface IRegistry<TTarget> : IReadOnlyRegistry<TTarget>
    {
        void Register<T>(T context) where T : TTarget;
    }
}
