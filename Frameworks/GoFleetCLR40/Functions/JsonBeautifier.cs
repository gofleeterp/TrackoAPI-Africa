using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;

namespace GoFleetCLR.Functions
{
    public class JsonBeautifier
    {
        private const string INDENT_STRING = "    ";

        [SqlFunction]
        public static SqlString IndentJson(string json)
        {
            // Put your code here
            var FormatedJson = FormatJson(json);
            return new SqlString(FormatedJson);
        }

        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();

                            foreach (var item in Enumerable.Range(0, ++indent)) sb.Append(INDENT_STRING);
                        }

                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();

                            foreach (var item in Enumerable.Range(0, --indent)) sb.Append(INDENT_STRING);
                        }

                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        var escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();

                            foreach (var item in Enumerable.Range(0, indent)) sb.Append(INDENT_STRING);
                        }

                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}