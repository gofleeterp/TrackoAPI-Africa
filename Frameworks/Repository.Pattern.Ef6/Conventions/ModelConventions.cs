using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Repository.Pattern.Ef6.Conventions
{
    public class DataTypePropertyAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<DataTypeAttribute>
    {
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, DataTypeAttribute attribute)
        {
            
            switch (attribute.DataType)
            {
                case DataType.Date:
                    configuration.HasColumnType("Date");
                    break;
                case DataType.DateTime:
                    configuration.HasColumnType("DateTime");
                    break;
                case DataType.Time:
                    configuration.HasColumnType("Time");
                    break;
                case DataType.Custom:
                    break;
                    case DataType.MultilineText:
                    configuration.IsUnicode(true);
                    break;
                case DataType.Text:
                    configuration.IsUnicode(true);
                    break;
            }
        }
    }
}
