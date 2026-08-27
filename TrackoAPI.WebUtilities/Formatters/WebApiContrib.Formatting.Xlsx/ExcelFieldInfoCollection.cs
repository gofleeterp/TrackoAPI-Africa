using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.WebUtilities.Formatters.WebApiContrib.Formatting.Xlsx
{
    public class ExcelFieldInfoCollection : KeyedCollection<string, ExcelFieldInfo>
    {
        protected override string GetKeyForItem(ExcelFieldInfo item)
        {
            return item.PropertyName;
        }
    }
}
