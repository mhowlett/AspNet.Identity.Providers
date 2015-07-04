// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system.
// For credits, copyright and license information refer to LICENSE.txt

// Note: this file was taken directly from Microsoft's EF implementation.

using System;


namespace AspNet.Identity.Providers.Common
{
    public class IdentityUserRole : IdentityUserRole<string> { }

    /// <summary>
    ///     EntityType that represents a user belonging to a role
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class IdentityUserRole<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     UserId for the user that is in the role
        /// </summary>
        public virtual TKey UserId { get; set; }

        /// <summary>
        ///     RoleId for the role
        /// </summary>
        public virtual TKey RoleId { get; set; }
    }
}
