namespace AM.Core
{
    public interface IRegistry<TTarget>
    {
        void Register<T>(T context) where T : TTarget;
        T Get<T>() where  T : TTarget;
        bool TryGet<T>(out T context) where T : TTarget;
    }
}
