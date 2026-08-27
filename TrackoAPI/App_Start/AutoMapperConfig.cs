using AutoMapper;
using System;
using System.Linq.Expressions;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;
using TrackoAPI.ViewModels.BMS;

namespace TrackoAPI
{
    public static class AutoMapperConfig
    {
        public static IMapper CreateMapper()
        {
            var auto_conf = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CnChallan, CnChallan>();
                cfg.CreateMap<vwBillPaymentLog, CNBillPaymentLog>();
                cfg.CreateMap<Voucher, Voucher>();
                cfg.CreateMap<CNBillLog, CNBillLog>().Ignore(x => x.Id);
                cfg.CreateMap<vwCNMultiMaterial, CNMultiMaterial>();
                cfg.CreateMap<vwEWayBill, CNEWayBill>();
            });
            return new Mapper(auto_conf);
        }
        public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
    this IMappingExpression<TSource, TDestination> map,
    Expression<Func<TDestination, object>> selector)
        {
            map.ForMember(selector, config => config.Ignore());
            return map;
        }
    }
}