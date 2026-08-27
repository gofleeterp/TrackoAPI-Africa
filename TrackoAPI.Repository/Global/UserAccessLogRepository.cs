using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoAPI.Repository
{
    public static class UserAccessLogRepository
    {
        public static void AddAuditLog(this IRepository<ApiRecordAccessLog> repo, ApiRecordAccessLog log)
        {
            log.ObjectState = ObjectState.Added;
            log.SessionId = Helper.SessionId();
            log.UserId = Helper.GetLoggedInUserId();
            log.TimeStamp = DateTime.Now;
            repo.Insert(log);
            repo.UOW.SaveChanges();
        }
        public static async Task AddAuditLogAsync(this IRepositoryAsync<ApiRecordAccessLog> repo, ApiRecordAccessLog log)
        {
            log.ObjectState = ObjectState.Added;
            log.SessionId = Helper.SessionId();
            log.UserId = Helper.GetLoggedInUserId();
            log.TimeStamp = DateTime.Now;
            repo.Insert(log);
            await repo.UOW.SaveChangesAsync();
        }
        public static void AddOrUpdateLog(this IRepository<ApiResourceAccessLog> repository, ApiResourceAccessLog log)
        {
            var existing =
                repository.Query(
                    x =>
                        x.ApplicationId == log.ApplicationId && x.ResourceId == log.ResourceId &&
                        x.ResourceType == log.ResourceType && x.UserId == log.UserId).Select().FirstOrDefault();
            if (existing!=null)
            {
                existing.LastAccessDateTime=DateTime.UtcNow;
                existing.Count++;
                existing.ObjectState=ObjectState.Modified;
                repository.Update(existing);
            }
            else
            {
                //TODO:Developer must set ApplicationId & UserId in Controller
                if (string.IsNullOrWhiteSpace(log.ApplicationId) || !(log.UserId > 0))
                {
                    throw new NullReferenceException("ApplicationId or UserId was Null");
                }
                log.ObjectState = ObjectState.Added;
                log.LastAccessDateTime=DateTime.Now;
                log.Count++;
                repository.Insert(log);
            }
        }

        public static long GetResourceId(this IRepository<ApiResourceAccessLog> repository, string resourceName, AclType resourceType)
        {
            long resourceId = 0;
            switch (resourceType)
            {
                case AclType.Entity:
                    resourceId = repository.GetRepository<Ledger>().Query(x => x.AccountName == resourceName).Select(x => x.Id).FirstOrDefault();
                    break;
                case AclType.UserControl:
                case AclType.MobileView:
                case AclType.Form:
                case AclType.Report:
                    resourceId = repository.GetRepository<ApiView>().Query(x => x.Name == resourceName).Select(x => x.Id).FirstOrDefault();
                    break;
                case AclType.Field:
                    resourceId = repository.GetRepository<ViewField>().Query(x => x.FieldType == resourceName).Select(x => x.Id).FirstOrDefault();
                    break;              
                    
                case AclType.Action:
                    break;
                case AclType.Office:
                    resourceId = repository.GetRepository<OfficeMaster>().Query(x => x.OfficeName == resourceName).Select(x => x.Id).FirstOrDefault();
                    break;
                case AclType.GenericMaster:
                    break;
                case AclType.UserDefinedReport:
                    resourceId = repository.GetRepository<UserDefinedReport>().Query(x => x.Name == resourceName).Select(x => x.Id).FirstOrDefault();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null);
            }
            return resourceId;
        }
    }
}
