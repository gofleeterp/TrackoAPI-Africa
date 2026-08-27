using Repository.Pattern.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
    public static class VoucherTypeGroupMappingRepository
    {
        public static IQueryable<Ledger> GetLedgersByFieldId(this IRepository<VoucherTypeGroupMapping> repository, long? voucherTypeid, long type, long? viewId)
        {
            IQueryable<VoucherTypeGroupMapping> vtgmap = repository.Queryable().Where(x => x.TypeId == type);
            if (voucherTypeid.GetValueOrDefault(0) > 0)
            {
                vtgmap = vtgmap.Where(x => x.VoucherTypeId == voucherTypeid);
            }
            if (viewId.GetValueOrDefault(0) > 0)
            {
                vtgmap = vtgmap.Where(x => x.ViewId == viewId);
            }
            if (!vtgmap.Any())
            {
                return null;
            }
            var ledgRepo = repository.GetRepository<Ledger>().Queryable();

            var include = new List<long>();
            var exclude = new List<long>();
            var groupids = new List<long>();
            var roleids = new List<long?>();
            foreach (var v in vtgmap.ToList())
            {
                if (v.GroupId.HasValue && !groupids.Contains(v.GroupId.Value))
                {
                    groupids.Add(v.GroupId.Value);
                }
                if (!string.IsNullOrWhiteSpace(v.Include))
                {
                    var ar1 = v.Include.Split(',');
                    foreach (var s in ar1)
                    {
                        long id;
                        if (long.TryParse(s, out id) && !include.Contains(id)) include.Add(id);
                    }
                }
                if (v.LedgerRoleId.HasValue)
                {
                    roleids.Add(v.LedgerRoleId.Value);
                }
                if (string.IsNullOrWhiteSpace(v.Exclude)) continue;
                var ar2 = v.Exclude.Split(',');
                foreach (var s in ar2)
                {
                    long id;
                    if (long.TryParse(s, out id) && !exclude.Contains(id)) exclude.Add(id);
                }
            }
            if (groupids.Any())
            {
                try
                {
                    var grpids =
                    repository.UOW.Context.AccountGroupChildren.Where(x => groupids.Contains(x.GrandParentId))
                        .Select(x => x.GroupId)
                        .Distinct()
                        .ToList();
                    if (grpids.Any())
                    {
                        groupids.AddRange(grpids);
                    }
                }
                catch (Exception ex)
                {
                }
                groupids = groupids.Distinct().ToList();
            }
            if (!groupids.Any() && !roleids.Any() && !include.Any() && !exclude.Any())
            {
                return ledgRepo;
            }
            if (!groupids.Any() && !roleids.Any() && !include.Any() && exclude.Any())
            {
                return ledgRepo.Where(x => !exclude.Contains(x.Id));
            }
            var rRepo = repository.GetRepository<LedgerRole>().Queryable();
            var query= (from l in ledgRepo
                    from r in rRepo.Where(x => x.LedgerId == l.Id).DefaultIfEmpty()
                    where !l.IsDefaulter && (groupids.Contains(l.GroupId.Value) || (r != null && roleids.Contains(r.RoleId) || include.Contains(l.Id)) && !exclude.Contains(l.Id))
                    select l).Distinct();
            //var applypermissionOnVTG=_
            //if()
            return query;
        }

        /// <exception cref="BusinessException">VoucherVisiblityFlag key not found in Configuration.</exception>
        public static IQueryable<Ledger> GetLedgersByVoucherTypeId(this IRepository<VoucherTypeGroupMapping> repository, long? voucherTypeid, long type, long? viewId)
        {
            var setting = repository.GetRepository<ApiConfiguration>().Find("VoucherVisiblityFlag");

            if (setting == null)
            {
                throw new BusinessException(ErrorCode.GLB103, "Key:VoucherVisiblityFlag");
            }
            IQueryable<VoucherTypeGroupMapping> vtgmap = repository.Queryable().Where(x => x.TypeId == type);
            if (voucherTypeid.GetValueOrDefault(0) > 0)
            {
                vtgmap = vtgmap.Where(x => x.VoucherTypeId == voucherTypeid);
            }
            if (viewId.GetValueOrDefault(0) > 0)
            {
                vtgmap = vtgmap.Where(x => x.ViewId == viewId);
            }
            if (!vtgmap.Any())
            {
                return null;
            }
            var ledgRepo = repository.GetRepository<Ledger>().Queryable();
            var include = new List<long>();
            var exclude = new List<long>();
            var groupids = new List<long>();
            var setingValue = setting.Value;
            var roleids = new List<long?>();
            foreach (var v in vtgmap)
            {
                if (setingValue != "0" && v.GroupId.HasValue && !groupids.Contains(v.GroupId.Value))
                {
                    groupids.Add(v.GroupId.Value);
                }
                if (!string.IsNullOrWhiteSpace(v.Include))
                {
                    var ar1 = v.Include.Split(',');
                    foreach (var s in ar1)
                    {
                        long id;
                        if (long.TryParse(s, out id) && !include.Contains(id)) include.Add(id);
                    }
                }
                if (v.LedgerRoleId.HasValue)
                {
                    roleids.Add(v.LedgerRoleId.Value);
                }
                if (string.IsNullOrWhiteSpace(v.Exclude)) continue;
                var ar2 = v.Exclude.Split(',');
                foreach (var s in ar2)
                {
                    long id;
                    if (long.TryParse(s, out id) && !exclude.Contains(id)) exclude.Add(id);
                }
            }
            if (include.Any())
            {
                ledgRepo = ledgRepo.Where(x => include.Contains(x.Id));
            }
            if (exclude.Any())
            {
                ledgRepo = ledgRepo.Where(x => !exclude.Contains(x.Id));
            }

            if (roleids.Any() && !groupids.Any())
            {
                //ledgRepo = ledgRepo.Where(x => roleids.Contains(x.AccountRoleId));
                ledgRepo = from l in ledgRepo
                           join r in repository.GetRepository<LedgerRole>().Queryable()
                               on l.Id equals r.LedgerId
                           where !l.IsDefaulter && roleids.Contains(r.RoleId)
                           select l;
            }
            else if (groupids.Any() && !roleids.Any())
            {
                ledgRepo = ledgRepo.Where(x => groupids.Contains(x.GroupId.Value));
            }
            else if (groupids.Any() && roleids.Any())
            {
                //ledgRepo = ledgRepo.Where(x => groupids.Contains(x.GroupId.Value)|| roleids.Contains(x.AccountRoleId));
                var rRepo = repository.GetRepository<LedgerRole>().Queryable();
                ledgRepo = from l in ledgRepo
                           from r in rRepo.Where(x => x.LedgerId == l.Id).DefaultIfEmpty()
                           where !l.IsDefaulter && groupids.Contains(l.GroupId.Value) || (r != null && roleids.Contains(r.RoleId))
                           select l;
            }
            return ledgRepo;
        }
    }
}