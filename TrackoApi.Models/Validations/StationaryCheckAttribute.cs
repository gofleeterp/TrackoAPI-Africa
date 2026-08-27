using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using TrackoApi.Models.Global;
using Unity;
using Unity.Config;

namespace TrackoApi.Models.Validations
{
    public class StationaryCheckAttribute:ValidationAttribute
    {
        private readonly string _primaryKeyName;

        public StationaryCheckAttribute(string primaryKeyName="Id")
        {
            _primaryKeyName = primaryKeyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            long id = 0;
            var idProperty = validationContext.ObjectType.GetProperty(_primaryKeyName);
            if (idProperty != null)
            {
                var idpropertyValue = idProperty.GetValue(validationContext.ObjectInstance, null)?.ToString();
                long.TryParse(idpropertyValue, out id);
                if (id > 0) return null;
            }

            var propertyInfo = validationContext.ObjectType.GetProperty("PageId");
            if (propertyInfo == null) return null;
            var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance, null)?.ToString();
            if (!string.IsNullOrWhiteSpace(propertyValue)&&long.TryParse(propertyValue,out var pageId)&&pageId>0)
            {
                var pageNo = value?.ToString();
                if(string.IsNullOrWhiteSpace(pageNo)||!IsPageAvialable(pageNo,pageId,id))
                //if (string.IsNullOrWhiteSpace(pageNo))
                {
                    return new ValidationResult($"Page Number {pageNo} has been consumed by someone else or it is out of book.");
                }
            }
            return null;
        }
        [Dependency]
        public IEntityTable<StationeryBookLog> Logs { get; set; }
        private bool IsPageAvialable(string pageNo,long pageid,long id)
        {
            var container = UnityCore.Container;
            try
            {
                var userService = Logs?? container?.Resolve<IEntityTable<StationeryBookLog>>();
                if (userService != null)
                {
                    var result = userService.Table.Any(x =>
                        (x.PageNo == pageNo && x.Id == pageid) || (x.Id == pageid && (x.NatureId == 1234 || x.NatureId == 1625)));
                    return result;
                }
            }
            catch (System.Exception)
            {
                //ignore for now
            }
            

            return true;
        }
    }
}
