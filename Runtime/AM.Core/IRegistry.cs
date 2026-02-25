namespace AM.Core
{
    public interface IRegistry<TTarget> where TTarget : class
    {
        void Register<T>(T context) where T : class, TTarget;
        T Get<T>() where  T : class, TTarget;
        bool TryGet<T>(out T context) where T : class, TTarget;
    }
}
