using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.StorageProviders.PostgreSQL
{
    public static class IdentityPostgreSQLServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the default identity system configuration for the specified User and Role types.
        /// </summary>
        /// <typeparam name="TUser">
        ///     The type representing a User in the system.
        /// </typeparam>
        /// <typeparam name="TRole">
        ///     The type representing a Role in the system.
        /// </typeparam>
        /// <param name="services">
        ///     The services available in the application.
        /// </param>
        /// <returns>
        ///     An <see cref="IdentityBuilder"/> for creating and configuring the identity system.
        /// </returns>
        public static void AddPostgreSQLDatabase<TDatabase>(
            this IServiceCollection services, TDatabase database)
            where TDatabase : PostgreSQLDatabase
        {
            services.TryAdd(new ServiceDescriptor(typeof(TDatabase), database));
        }
    }
}
