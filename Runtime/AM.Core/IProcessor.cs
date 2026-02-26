namespace AM.Core
{
    public interface IProcessor
    {
        void Initialize(object settingRegistry, object contextRegistry);
        void Process();

    }

    public interface IProcessor<TSetting, TContext> : IProcessor
        where TSetting : ISetting
        where TContext : IContext
    {
        void Initialize(
            Registry<TSetting> settingRegistry,
            Registry<TContext> contextRegistry);
    }
}