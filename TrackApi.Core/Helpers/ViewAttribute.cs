using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Globalization;

namespace TrackoApi.Core.Helpers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewTypeAttribute :Attribute
    {
        public ViewTypeAttribute(string viewName)
        {
            ViewName = viewName;
        }

        public string ViewName { get;}
    }
}