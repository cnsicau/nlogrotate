namespace logrotate
{
    public abstract class LogRotateOptionsBuilderProvider
    {
        public abstract LogRotateOptionsBuilder[] CreateBuilders();
    }
}
