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
    public interface IReportParameterService : IService<ReportParameter>
    {
    }
    public class ReportParameterService : Service<ReportParameter>, IReportParameterService
    {
        private readonly IRepositoryAsync<ReportParameter> _repository;
        public ReportParameterService(IRepositoryAsync<ReportParameter> repository) : base(repository)
        {
            _repository = repository;
        }
    }
    public interface ICustomReportParameterService : IService<UserDefinedReportParameter>
    {
    }
    public class CustomReportParameterService : Service<UserDefinedReportParameter>, ICustomReportParameterService
    {
        private readonly IRepositoryAsync<UserDefinedReportParameter> _repository;
        public CustomReportParameterService(IRepositoryAsync<UserDefinedReportParameter> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
