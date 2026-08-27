using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

using TrackoAPI.ViewModels.Global;

namespace Tenant.Models.CRM
{
    public class WorkItemAuditableEntity: Entity
        {
            [Column("cUserId"), AuditIgnore]
            public long cUserId { get; set; }
            [Column("cDOE"), AuditIgnore]
            public DateTime cDOE { get; set; }
            [Column("mUserId"), AuditIgnore]
            public long? mUserId { get; set; }
            [Column("mDOE"), AuditIgnore]
            public DateTime? mDOE { get; set; }
            public bool IsPublic { get; set; } = false;
            public bool IsDelete { get; set; } = false;
            private List<JsonDataEntity> _dt;
            public List<JsonDataEntity> Data
            {
                //get => _dt==null?(string.IsNullOrWhiteSpace(ExtraProperties)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties)): _dt;
                get
                {
                    try
                    {
                        if (MetaData == "{}") MetaData = "[]";
                        return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(MetaData ?? (MetaData = "[]")));
                    }
                    catch
                    {
                        return _dt ?? (_dt = new List<JsonDataEntity>());
                    }

                }
                set
                {
                    _dt = value;
                    MetaData = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
                }


            }
            [IsJsonValidate("Meta should contain valid json array data",AllowedJsonToken.Array)]
            public string MetaData { get; set; }

            public void DeleteAndAdd(JsonDataEntity entity)
            {
                try
                {
                    if ((MetaData ?? "{}") == "{}") MetaData = "[]";
                    if (_dt == null)
                    {
                        _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((MetaData ?? (MetaData = "[]")));
                    }

                    _dt.RemoveAll(x => x.DataName == entity.DataName);
                    _dt.Add(entity);
                    MetaData = JsonConvert.SerializeObject(_dt);
                }
                catch
                {
                    MetaData = "[]";
                }
            }
        }
    
}
