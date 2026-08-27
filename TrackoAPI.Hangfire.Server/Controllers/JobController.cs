using System.Diagnostics;
using System.Web.Http;
using HangfireService;

namespace TrackoAPI.Hangfire.Server.Controllers
{
    /**
     * 
        Hangfire使用方法：
        //立即执行
        Hangfire.BackgroundJob.Enqueue(() => hangfireTask.AddMethodLog("a"));
        //延迟
        Hangfire.BackgroundJob.Schedule(() => hangfireTask.AddMethodLog("a"),TimeSpan.FromDays(1));
        //循环
        Hangfire.RecurringJob.AddOrUpdate(() => hangfireTask.AddMethodLog("a"),Hangfire.Cron.Daily);
        //连续(通过将多个后台任务链接在一起来定义复杂的工作流)
        var task_a = Hangfire.BackgroundJob.Enqueue(() => hangfireTask.AddMethodLog("a"));
        Hangfire.BackgroundJob.ContinueWith(task_a, () => hangfireTask.AddMethodLog("a"));
     * */
    [RoutePrefix("api/v1/job")]
    public class JobController : ApiController
    {
        [Route("test")]
        [HttpGet]
        public UnifiedResultEntity<string> GetInfo()
        {
            UnifiedResultEntity<string> response_entity = new UnifiedResultEntity<string>();
            response_entity.msg = 1;
            response_entity.msgbox = "success";
            Trace.Write("接口已通");
            return response_entity;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("add/demo")]
        public UnifiedResultEntity<string> AddJob(string val)
        {
            UnifiedResultEntity<string> response_entity = new UnifiedResultEntity<string>();
            HangfireService.HangfireExecute.Demo demo = new HangfireService.HangfireExecute.Demo();
            var job_id = demo.Add(val);

            response_entity.msg = 1;
            response_entity.msgbox = "OK";
            response_entity.data = job_id;
            return response_entity;
        }

    }
}
