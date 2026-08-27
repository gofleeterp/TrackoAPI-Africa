using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.HangfireExecute
{
    /// <summary>
    /// 演示添加任务
    /// </summary>
    public class Demo
    {
        public string Add(string val)
        {
            var wr = "执行任务，打印参数:" + val;
            return Hangfire.BackgroundJob.Enqueue(() => System.Diagnostics.Trace.WriteLine(wr));
        }
    }
}
