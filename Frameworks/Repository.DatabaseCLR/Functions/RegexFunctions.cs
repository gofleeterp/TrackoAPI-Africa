using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

namespace Repository.DatabaseCLR.Functions
{
    /// <summary>
    ///     compile using:
    ///     C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:library RegexFunctions.cs
    ///     https://docs.microsoft.com/en-us/sql/relational-databases/clr-integration-database-objects-user-defined-functions/clr-scalar-valued-functions
    ///     https://github.com/mattmc3/mssql-regex-clr/blob/master/RegexFunctions.cs
    ///     CREATE ASSEMBLY RegexFunctions from 'C:\Program Files\Microsoft SQL Server\CLR\RegexFunctions.dll' WITH
    ///     PERMISSION_SET = SAFE;
    /// </summary>
    public class RegexFunctions
    {
        private const RegexOptions Xms = RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline |
                                         RegexOptions.Singleline;

        private const RegexOptions Xmsi = Xms | RegexOptions.IgnoreCase;

        // CREATE FUNCTION IsRegexMatch(@input NVARCHAR(4000), @pattern NVARCHAR(4000)) RETURNS BIT AS
        // EXTERNAL NAME RegexFunctions.RegexFunctions.IsRegexMatch
        // GO
        [SqlFunction]
        public static SqlBoolean IsRegexMatch(SqlString input, SqlString pattern)
        {
            if (input.IsNull || pattern.IsNull) return SqlBoolean.Null;
            return Regex.IsMatch(input.Value, pattern.Value, Xms);
        }

        // CREATE FUNCTION RegexReplace(@input NVARCHAR(4000), @pattern NVARCHAR(4000), @replacement NVARCHAR(4000)) RETURNS NVARCHAR(4000) AS
        // EXTERNAL NAME RegexFunctions.RegexFunctions.RegexReplace
        // GO
        [SqlFunction]
        public static SqlString RegexReplace(SqlString input, SqlString pattern, SqlString replacement)
        {
            if (input.IsNull || pattern.IsNull || replacement.IsNull)
                return SqlString.Null;
            return Regex.Replace(input.Value, pattern.Value, replacement.Value, Xms);
        }

        // CREATE FUNCTION RegexMatchGroup(@input NVARCHAR(4000), @pattern NVARCHAR(4000), @groupNum int) RETURNS NVARCHAR(4000) AS
        // EXTERNAL NAME RegexFunctions.RegexFunctions.RegexMatchGroup
        // GO
        [SqlFunction]
        public static SqlString RegexMatchGroup(SqlString input, SqlString pattern, SqlInt32 groupNum)
        {
            if (input.IsNull || pattern.IsNull || groupNum.IsNull) return SqlString.Null;
            if (groupNum.Value < 0) return SqlString.Null;
            var re = new Regex(pattern.Value, Xms);
            var m = re.Match(input.Value);
            if (!m.Success || m.Groups.Count < groupNum.Value)
                return SqlString.Null;
            return m.Groups[groupNum.Value].Value;
        }

        // CREATE FUNCTION RegexIndex(@input NVARCHAR(4000), @pattern NVARCHAR(4000)) RETURNS INT AS
        // EXTERNAL NAME RegexFunctions.RegexFunctions.RegexIndex
        // GO
        [SqlFunction]
        public static SqlInt32 RegexIndex(SqlString input, SqlString pattern)
        {
            if (input.IsNull || pattern.IsNull) return SqlInt32.Null;
            var re = new Regex(pattern.Value, Xms);
            var m = re.Match(input.Value);
            if (!m.Success) return 0;
            return m.Index + 1; // SQL indexes strings by 1
        }
    }
}