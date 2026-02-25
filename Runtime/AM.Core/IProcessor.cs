namespace AM.Core
{
    public interface IProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
        void Initialize(Registry<TSetting> settingRegistry, Registry<TContext> contextRegistry);

        void Process();
    }
}