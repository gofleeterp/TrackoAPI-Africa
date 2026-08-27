using System;
using System.Data.Entity;

namespace Repository
{
    public static class MyDbFunctions
    {
        [DbFunction("CodeFirstDatabaseSchema", "DateTimeToString")]
        public static string DateTimeToString(DateTime date)
        {
            return date.ToString("yyyy-MMM-dd HH:mm");
        }
        private const string ErrorMessage = "{0} can be used only in linq to entity query";
        [DbFunction("CodeFirstDatabaseSchema", "GetLevel")]
        public static int GetLevel(byte[] node)
        {
            throw new NotSupportedException(string.Format(ErrorMessage, nameof(GetLevel)));
        }
        [DbFunction("CodeFirstDatabaseSchema", "IsDescendantOf")]
        public static byte IsDescendantOf(byte[] node, byte[] parent)
        {
            throw new NotSupportedException(string.Format(ErrorMessage, nameof(IsDescendantOf)));
        }
        
    }
}
