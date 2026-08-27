using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Base
{
    public interface IEntity
    {
        [Key, Column("Id",Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        long Id { get; set; }
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
    public class Entity : IEntity
    {
       // [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual long Id { get; set; }
        [NotMapped]
        public ObjectState ObjectState { get; set; }
    }
}
