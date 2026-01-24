using System.Windows;

namespace Belpost.Auth
{
    public partial class App : Application
    {
        public static AppDb? Db { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Db = new AppDb();
            Db.Database.EnsureCreated();
        }
    }
}
