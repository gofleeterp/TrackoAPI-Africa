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
    public interface IPostalAddressService : IService<PostalAddress>
    {
    }
    public class PostalAddressService : Service<PostalAddress>, IPostalAddressService
    {
        private readonly IRepositoryAsync<PostalAddress> _repository;
        public PostalAddressService(IRepositoryAsync<PostalAddress> repository) : base(repository)
        {
            _repository = repository;
        }

        public override PostalAddress Insert(PostalAddress entity)
        {
            if (entity.CountryId.GetValueOrDefault(0) == 0)
            {
                entity.CountryId = null;
            }
            return base.Insert(entity);
        }

        public override void Update(PostalAddress entity)
        {
            if (entity.CountryId.GetValueOrDefault(0) == 0)
            {
                entity.CountryId = null;
            }
            base.Update(entity);
        }
    }
}
