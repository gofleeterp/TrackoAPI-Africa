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
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ILedgerPrintFormatService : IService<LedgerPrintFormat>
    {
    }
    public class LedgerPrintFormatService : Service<LedgerPrintFormat>, ILedgerPrintFormatService
    {
        private readonly IRepositoryAsync<LedgerPrintFormat> _repository;
        public LedgerPrintFormatService(IRepositoryAsync<LedgerPrintFormat> repository) : base(repository)
        {
            _repository = repository;
        }

        public override LedgerPrintFormat Insert(LedgerPrintFormat entity)
        {
            if (entity.IsDefault)
            {
                var existingdefaultFormats = this.Queryable()
                    .Where(x => x.LedgerId == entity.LedgerId && x.OfficeId == entity.OfficeId &&
                                x.PrintFormatId == entity.PrintFormatId && x.IsDefault)
                    .ToList();
                foreach (var format in existingdefaultFormats)
                {
                    format.IsDefault = false;
                    format.ObjectState=ObjectState.Modified;
                }
            }
            return base.Insert(entity);
        }

        public override void Update(LedgerPrintFormat entity)
        {
            if (entity.IsDefault)
            {
                var existingdefaultFormats = this.Queryable()
                    .Where(x => x.LedgerId == entity.LedgerId && x.OfficeId == entity.OfficeId &&
                                x.PrintFormatId == entity.PrintFormatId && x.IsDefault&&x.Id!=entity.Id)
                    .ToList();
                foreach (var format in existingdefaultFormats)
                {
                    format.IsDefault = false;
                    format.ObjectState = ObjectState.Modified;
                }
            }
            base.Update(entity);
        }

        public override void Patch(LedgerPrintFormat entity)
        {
            if (entity.IsDefault)
            {
                var existingdefaultFormats = this.Queryable()
                    .Where(x => x.LedgerId == entity.LedgerId && x.OfficeId == entity.OfficeId &&
                                x.PrintFormatId == entity.PrintFormatId && x.IsDefault && x.Id != entity.Id)
                    .ToList();
                foreach (var format in existingdefaultFormats)
                {
                    format.IsDefault = false;
                    format.ObjectState = ObjectState.Modified;
                }
            }
            base.Patch(entity);
        }
    }
}
