using System.Windows;

namespace Belpost.Auth
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var db = new AppDb())
            {
                db.Database.EnsureCreated();
            }
        }
    }
}
