using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Resources;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Core.Helpers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlSqlType : Attribute
    {
    }
}
