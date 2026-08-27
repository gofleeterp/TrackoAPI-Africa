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
using TrackoApi.Core.Helpers;
using System.IO;

namespace TrackoApi.Service
{
    public interface IDocumetsService : IService<ApiFile>
    {
        string NewFileName(long id);
    }
    public class DocumetsService : Service<ApiFile>, IDocumetsService
    {
        private readonly IRepositoryAsync<ApiFile> _repository;
        public DocumetsService(IRepositoryAsync<ApiFile> repository) : base(repository)
        {
            _repository = repository;
        }

        public override ApiFile Insert(ApiFile entity)
        {
            var nature = _repository.GetRepository<FileUploadNature>().Queryable().Where(x => x.Id == entity.NatureId).Select(x=>new
            {
                x.Id,
                RelatedCode=x.fk_Type.ConstantAbbr,
                NatureCode=x.Code,
                RelatedId=x.TypeId
            }).FirstOrDefault();
            if (nature != null)
            {
                entity.Name = $"{nature.RelatedCode}_{nature.NatureCode}";
                entity.RelatedId = nature.RelatedId;
            }
            return base.Insert(entity);
        }
        public string NewFileName(long id)
        {
            var path = _repository.Queryable().Where(x => x.Id == id).Select(x => new { x.Name,x.NatureId }).FirstOrDefault();
            var nature = _repository.GetRepository<FileUploadNature>().Queryable().Where(x => x.Id == path.NatureId).Select(x => new
            {
                x.Id,
                RelatedCode = x.fk_Type.ConstantAbbr,
                NatureCode = x.Code,
                RelatedId = x.TypeId
            }).FirstOrDefault();
            return $"{nature.RelatedCode.Trim()}_{nature.NatureCode.Trim()}_{path.Name.Trim()}";
        }
        public string NewFileNameOld(long id)
        {
            var configuredPath = Utilities.FileUploadFolder();
            var uploadPath = Utilities.MapPathToServer(configuredPath);

            var tenantid = _repository.GetRepository<ApiAppClient>().Queryable().Select(x => x.ClientKey).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantid))
            {
                uploadPath = Path.Combine(uploadPath, tenantid);
            }
            var path = _repository.Queryable().Where(x => x.Id == id).Select(x => new { x.Name, x.ServerFilePath }).FirstOrDefault();
            var serverpath = path.ServerFilePath.Replace(uploadPath, "").Replace("\\", "_");
            if (serverpath.StartsWith("_")) serverpath = serverpath.Substring(1);
            return string.IsNullOrWhiteSpace(path.ServerFilePath) ? path.Name : (serverpath);
        }
    }
}
