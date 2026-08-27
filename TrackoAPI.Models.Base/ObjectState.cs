namespace TrackoApi.Models.Base
{
    public enum ObjectState
    {
        Unchanged,
        Added,
        Modified,
        Deleted
    }
    
}

namespace TrackoApi.Models.Global
{
    public enum AccessType
    {
        Viewed = 1,
        Created = 2,
        Updated = 3,
        Deleted = 4
    }

}