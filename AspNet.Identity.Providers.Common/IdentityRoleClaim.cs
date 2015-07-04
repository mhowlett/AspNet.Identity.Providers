// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system.
// For credits, copyright and license information refer to LICENSE.txt

// Note: this file was taken directly from Microsoft's EF implementation.

using System;


namespace AspNet.Identity.Providers.Common
{
    public class IdentityRoleClaim : IdentityRoleClaim<string> { }

    /// <summary>
    ///     EntityType that represents one specific role claim
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class IdentityRoleClaim<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     Primary key
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        ///     User Id for the role this claim belongs to
        /// </summary>
        public virtual TKey RoleId { get; set; }

        /// <summary>
        ///     Claim type
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        ///     Claim value
        /// </summary>
        public virtual string ClaimValue { get; set; }
    }
}