using System.ComponentModel;
using System.Configuration.Install;

namespace TrackoAPI.Hangfire.Server
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
