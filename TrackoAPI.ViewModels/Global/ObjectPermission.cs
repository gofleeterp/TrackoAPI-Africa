using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.Global
{
    public class ObjectPermission
    {
        public ObjectPermission()
        {
            Reset();
        }

        private void Reset()
        {
            Read = false;
            Write = false;
            Update = false;
            Delete = false;
            Deny = false;
        }
        public enum Right
        {
            Read = 1,
            Write = 3,
            Update = 7,
            Delete = 15,
            Deny = 31,

        }
        public int GetPermissionId()
        {
            if (Deny) return 31;
            if (Delete) return 15;
            if (Update) return 7;
            if (Write) return 3;
            if (Read) return 1;
            return 0;
        }

        public ObjectPermission BuildPermission(int value = 0)
        {
            Reset();
            Deny = value >= 31;
            if (Deny) return this;
            Delete = value >= 15;
            Update = value >= 7;
            Write = value >= 3;
            Read = value >= 1;
            return this;
        }

        public Right Max(int value)
        {
            BuildPermission(value);
            return Max();
        }
        public Right Max()
        {
            if (Deny) return Right.Deny;
            if (Delete) return Right.Delete;
            if (Update) return Right.Update;
            if (Write) return Right.Write;
            if (Read) return Right.Read;
            return Right.Deny;
        }
        public bool Read { get; set; }
        public bool Write { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Deny { get; set; }
        public Right MaxPermission { get; set; }
    }
}
