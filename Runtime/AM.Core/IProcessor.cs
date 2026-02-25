namespace AM.Core
{
    public interface IProcessor
    {
        void Initialize(Registry<ISetting> settingRegistry, Registry<IContext> contextRegistry);

        void Process();
    }
}