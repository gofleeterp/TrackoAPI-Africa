using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IPrintFormatService : IService<PrintFormatMaster>
    {
    }
    public class PrintFormatService : Service<PrintFormatMaster>, IPrintFormatService
    {
        private readonly IRepositoryAsync<PrintFormatMaster> _repository;
        public PrintFormatService(IRepositoryAsync<PrintFormatMaster> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
