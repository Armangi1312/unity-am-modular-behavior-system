namespace AM.Core
{
    public interface IProcessor
    {
    }

    public interface IProcessor<TSetting, TContext> : IProcessor
        where TSetting : ISetting
        where TContext : IContext
    {
        void Initialize(IReadOnlyRegistry<TSetting> settingRegistry, IReadOnlyRegistry<TContext> contextRegistry);
        void Process();
    }
}