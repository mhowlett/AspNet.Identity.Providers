// A plain text filesystem provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.Providers.Filesystem
{
    /// <summary>
    ///     Default services
    /// </summary>
    public class IdentityServices
    {
        public static IEnumerable<ServiceDescriptor> GetDefaultServices(Type userType, Type roleType, Type contextType)
        {
            Type userStoreType;
            Type roleStoreType;

            userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, contextType);
            roleStoreType = typeof(RoleStore<,,>).MakeGenericType(userType, roleType, contextType);

            var services = new List<ServiceDescriptor>();
            services.Add(
                new ServiceDescriptor(
                typeof(IUserStore<>).MakeGenericType(userType),
                userStoreType, ServiceLifetime.Scoped));
            services.Add(
                new ServiceDescriptor(
                typeof(IRoleStore<>).MakeGenericType(roleType),
                roleStoreType, ServiceLifetime.Scoped));
            return services;
        }
    }
}
