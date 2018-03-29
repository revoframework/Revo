using System;

namespace Revo.DataAccess.Entities
{
    [Flags]
    public enum EntityState
    {
        //
        // Summary:
        //     The entity is not being tracked by the context. An entity is in this state immediately
        //     after it has been created with the new operator or with one of the System.Data.Entity.DbSet
        //     Create methods.
        Detached = 1,
        //
        // Summary:
        //     The entity is being tracked by the context and exists in the database, and its
        //     property values have not changed from the values in the database.
        Unchanged = 2,
        //
        // Summary:
        //     The entity is being tracked by the context but does not yet exist in the database.
        Added = 4,
        //
        // Summary:
        //     The entity is being tracked by the context and exists in the database, but has
        //     been marked for deletion from the database the next time SaveChanges is called.
        Deleted = 8,
        //
        // Summary:
        //     The entity is being tracked by the context and exists in the database, and some
        //     or all of its property values have been modified.
        Modified = 16
    }
}
