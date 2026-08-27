using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using TrackoApi.Models.Global;
using Unity;
using Unity.Config;

namespace TrackoApi.Models.Validations
{
    public class AutoStationaryCheckAttribute<TEntity> :ValidationAttribute
    {
        private readonly string _primaryKeyName;

        public StationaryCheckAttribute<TEntity>(string primaryKeyName="Id")
        {
            _primaryKeyName = primaryKeyName;
        }
        //public override bool IsValid(object value)
        //{
            //var pageNo = (string)value;
            //return StationaryService.Table.Any(x => x.PageNo == pageNo);
        //}

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
            if (propertyInfo == null)
            {
                var fieldidProperty = validationContext.ObjectType.GetProperty("AutoStationaryFieldId");
                if (fieldidProperty != null)
                {
                    var fieldidpropertyValue = fieldidProperty.GetValue(validationContext.ObjectInstance, null)?.ToString();
                    long.TryParse(fieldidpropertyValue, out id);
                    if (id > 0)
                    {

                    }
                }
            }
            var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance, null)?.ToString();
            if (!string.IsNullOrWhiteSpace(propertyValue)&&long.TryParse(propertyValue,out var pageId)&&pageId>0)
            {
                var pageNo = value?.ToString();
                if(string.IsNullOrWhiteSpace(pageNo)||!IsPageAvialable(pageNo,pageId,id))
                {
                    return new ValidationResult($"Page Number {pageNo} has been consumed by someone else or it is out of book.");
                }
            }
            return null;
        }
        [Dependency]
        public IEntityTable<StationeryBookLog> ET { get; set; }
        private bool IsPageAvialable(string pageNo,long pageid,long id)
        {
            var container = UnityCore.Container;
            try
            {
                var tableService = ET ??(ET= container?.Resolve<IEntityTable<StationeryBookLog>>());
                if (tableService != null)
                {
                    var result = tableService.Table.Any(x =>
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
        private bool GetNewPageByFieldId(long id)
        {
            var container = UnityCore.Container;
            try
            {
                var tableService = ET ?? (ET = container?.Resolve<IEntityTable<StationeryBookLog>>());
                if (tableService != null)
                {
                    var result = tableService.Table.Any(x =>
                        (x.fk) || (x.Id == pageid && (x.NatureId == 1234 || x.NatureId == 1625)));
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
    public class StationaryPageNo
    {
        public long PageId { get; set; }
        public string PageNo { get; set; }
        public long NatureId { get; set; }
    }
}
