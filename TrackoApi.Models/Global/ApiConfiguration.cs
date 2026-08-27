using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackoApi.Models.Global
{
    public class ApiConfiguration
    {
        [MaxLength(50),Key,DatabaseGenerated(DatabaseGeneratedOption.None),Column("Id")]
        public string Key { get; set; }
        public string Value { get; set; }
        [MaxLength(500)]
        public string Options { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsReserved { get; set; }
        public ApiConfiguration()
        {

        }
        public ApiConfiguration(string key,string value)
        {
            Key = key;
            Value = value;
        }
    }
    [Table("mClientConfiguration")]
    public class ClientConfiguration
    {
        [MaxLength(50), Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column("Id")]
        public string Id { get; set; }
        public string ConfigValue { get; set; }
        [MaxLength(500)]
        public string Options { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsReserved { get; set; }
    }

}
