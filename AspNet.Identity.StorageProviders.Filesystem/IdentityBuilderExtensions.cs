// A plain text filesystem provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using Microsoft.AspNet.Identity;
using Microsoft.Framework.DependencyInjection;
using AspNet.Identity.StorageProviders.Common;


namespace AspNet.Identity.StorageProviders.Filesystem
{
    public static class BuilderExtensions
    {
        public static IdentityBuilder AddFilesystemStores<TUser, TRole, TContext>(this IdentityBuilder builder)
                    where TUser : IdentityUser
                    where TRole : IdentityRole
                    where TContext : FilesystemContext<TUser, TRole>
        {
            builder.Services.TryAdd(IdentityServices.GetDefaultServices(builder.UserType, builder.RoleType, typeof(TContext)));
            return builder;
        }
    }
}