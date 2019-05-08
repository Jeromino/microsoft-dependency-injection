using Microsoft.Extensions.DependencyInjection;
using System;
using Unity.Lifetime;
using Unity.Microsoft.DependencyInjection.Lifetime;

namespace Unity.Microsoft.DependencyInjection
{
    public class ServiceProvider : IServiceProvider, IServiceScopeFactory, IServiceScope
    {
        private readonly IUnityContainer _container;
        private bool _disposed;

        internal ServiceProvider(IUnityContainer container)
        {
            _container = container;
            _container.RegisterInstance<IServiceScope>(this, new ExternallyControlledLifetimeManager());
            _container.RegisterInstance<IServiceProvider>(this, new ServiceProviderLifetimeManager(this));
            _container.RegisterInstance<IServiceScopeFactory>(this, new ExternallyControlledLifetimeManager());
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch  { /* Ignore */}

            return null;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceProvider(_container.CreateChildContainer());
        }

        IServiceProvider IServiceScope.ServiceProvider => this;

        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new ServiceProvider(new UnityContainer()
                .AddExtension(new MdiExtension())
                .AddServices(services));
        }

        public static explicit operator UnityContainer(ServiceProvider c)
        {
            return (UnityContainer)c._container;
        }

        public void Dispose()
        {
            lock (_container)
            {
                if (_disposed)
                {
                    return;
                }

                _container.Dispose();
                _disposed = true;
            }
        }
    }
}
