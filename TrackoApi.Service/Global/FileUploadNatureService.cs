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
    public interface IFileUploadNatureService : IService<FileUploadNature>
    {
    }
    public class FileUploadNatureService : Service<FileUploadNature>, IFileUploadNatureService
    {
        private readonly IRepositoryAsync<FileUploadNature> _repository;
        public FileUploadNatureService(IRepositoryAsync<FileUploadNature> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
