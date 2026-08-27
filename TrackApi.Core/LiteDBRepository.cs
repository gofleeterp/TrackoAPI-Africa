using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Core
{
    public class LiteDBRepository
    {
        public LiteDBRepository()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory ;
            _repository = new LiteRepository(new ConnectionString($"Filename={(Path.Combine(dir,"LiteDb.db"))};Cache Size=50;Mode=Shared;Initial Size=5000;Flush=true;"));
        }
        public static LiteDBRepository Instance = _instance ?? (_instance=new LiteDBRepository());
        private static LiteDBRepository _instance;
        private LiteRepository _repository;
        public ErrorMessageStore GetMessage(string id)
        {
            try
            {
                var tenantid = Helpers.Helper.LoggedInTenantId;
                if (!string.IsNullOrWhiteSpace(tenantid))
                {
                    return _repository.Query<ErrorMessageStore>("errormessages").Where(x => x.TenantId == tenantid&&x.MessageId==id).FirstOrDefault();
                }
                else
                {
                    return _repository.Query<ErrorMessageStore>("errormessages").Where(x =>x.MessageId == id).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                return new ErrorMessageStore();
            }
            
        }
        public void UpsertMessage(ErrorMessageStore errorMessage)
        {
            _repository.Upsert(errorMessage, "errormessages");
        }
    }
    public class ErrorMessageStore
    {
        public string MessageId { get; set; }
        public string TenantId { get; set; }
        public string SearchPattern { get; set; }
        public string Message { get; set; }
        public string CustomMessage { get; set; }
    }
}
