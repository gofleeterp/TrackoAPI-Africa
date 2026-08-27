using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ICurrencyConversionService : IService<CurrencyConversion>
    {
        IQueryable<CurrencyConversion> GetAllCurrencyConversionList(int id);
    }
    public class CurrencyConversionService : Service<CurrencyConversion>, ICurrencyConversionService
    {
        private readonly IRepositoryAsync<CurrencyConversion> _repository;
        public CurrencyConversionService(IRepositoryAsync<CurrencyConversion> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<CurrencyConversion> GetAllCurrencyConversionList(int brandid)
        {
            return _repository.GetAllCurrencyConversionList(brandid);
        }
    }
}
