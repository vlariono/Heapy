using System;
using Heapy.Core.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Heapy.Core.DI
{
    public static class ServiceCollectionExtension
    {
        public static void AddPrivateHeap(this IServiceCollection services,Func<IUnmanagedHeap> heapFactory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (heapFactory == null)
            {
                throw new ArgumentNullException(nameof(heapFactory));
            }

            services.AddScoped<IUnmanagedHeap>(provider => heapFactory.Invoke());
        }
    }
}