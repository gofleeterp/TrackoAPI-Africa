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
    public interface IReportCustomizationService : IService<ReportCustomization>
    {
    }
    public class ReportCustomizationService : Service<ReportCustomization>, IReportCustomizationService
    {
        private readonly IRepositoryAsync<ReportCustomization> _repository;
        public ReportCustomizationService(IRepositoryAsync<ReportCustomization> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
