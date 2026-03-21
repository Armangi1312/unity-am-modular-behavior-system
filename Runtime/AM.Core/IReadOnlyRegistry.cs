namespace AM.Core
{
    public interface IReadOnlyRegistry<TTarget>
    {
        T Get<T>() where T : TTarget;
        bool TryGet<T>(out T value) where T : TTarget;
    }
}
