using System.ServiceProcess;

namespace HangfireService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new HangfireService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
