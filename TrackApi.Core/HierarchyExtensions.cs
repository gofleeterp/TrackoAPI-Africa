using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Core
{
    public static class HierarchyExtensions
    {
        public static byte[] ToByteArray(this SqlHierarchyId id)
        {
            if (id.IsNull)
            {
                return null;
            }
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    id.Write(writer);
                    return stream.ToArray();
                }
            }
        }
        public static SqlHierarchyId ToSqlHierarchyId(this string value)
        {
            return string.IsNullOrEmpty(value)
            ? SqlHierarchyId.Null
            : SqlHierarchyId.Parse(value);
        }
        public static SqlHierarchyId ToSqlHierarchyId(this byte[] value)
        {
            if (value == null)
            {
                return SqlHierarchyId.Null;
            }
            using (var stream = new MemoryStream(value, false))
            {
                using (var reader = new BinaryReader(stream))
                {
                    SqlHierarchyId id;
                    id.Read(reader);
                    return id;
                }
            }
        }
    }
}
