using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Core.Helpers
{
    public enum AllowedJsonToken
    {
        Array,Object,Both
    }
    public class IsJsonValidateAttribute : ValidationAttribute
    {
        private readonly AllowedJsonToken _allowedType;

        public IsJsonValidateAttribute(string errorMessage, AllowedJsonToken allowedJson) : base(errorMessage)
        {
            _allowedType = allowedJson;
        }
        public override bool IsValid(object value)
        {
            string strInput = value.ToString();
            if (string.IsNullOrWhiteSpace(strInput)) { return true; }
            strInput = strInput.Trim();
            if(_allowedType==AllowedJsonToken.Array)
            {
                try
                {
                    var obj = JArray.Parse(strInput);
                    return true;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else if(_allowedType == AllowedJsonToken.Object)
            {
                try
                {
                    var obj = JObject.Parse(strInput);
                    return true;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
        (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("Value for '{0}' is not an allowed value", name);
        }
    }
}
