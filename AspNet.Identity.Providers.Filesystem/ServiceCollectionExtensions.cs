// A plain text filesystem provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using Microsoft.Framework.DependencyInjection;
using AspNet.Identity.Providers.Common;


namespace AspNet.Identity.Providers.Filesystem
{
    public static class ServiceCollectionExtensions
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
        public static void AddIdentityFilesystemContext<TContext, TUser, TRole>(
            this IServiceCollection services, TContext context)
            where TUser : IdentityUser
            where TRole : IdentityRole
            where TContext : FilesystemContext<TUser, TRole>
        {
            services.TryAdd(new ServiceDescriptor(typeof(TContext), context));
        }
    }
}
