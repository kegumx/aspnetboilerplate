using System.Data.Entity.Infrastructure.Interception;
using System.Reflection;
using Abp.Collections.Extensions;
using Abp.EntityFramework.Dependency;
using Abp.EntityFramework.Repositories;
using Abp.EntityFramework.SoftDeleting;
using Abp.EntityFramework.Uow;
using Abp.Modules;
using Abp.Reflection;
using Abp.Dependency;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;

namespace Abp.EntityFramework
{
    /// <summary>
    /// This module is used to implement "Data Access Layer" in EntityFramework.
    /// </summary>
    internal static class AbpEntityFrameworkRegistrar
    {
        public static ILogger Logger { get; set; }

        private static ITypeFinder _typeFinder;

        private static IIocManager IocManager;

        public static void Initialize(IIocManager iocManager)
        {
            IocManager = iocManager;
            Logger = iocManager.IocContainer.Resolve<ILogger>();
            _typeFinder = iocManager.IocContainer.Resolve<ITypeFinder>();

            iocManager.IocContainer.Register(
                Component.For(typeof(IDbContextProvider<>))
                    .ImplementedBy(typeof(UnitOfWorkDbContextProvider<>))
                    .LifestyleTransient()
                );

            RegisterGenericRepositories();

            DbInterception.Add(new SoftDeleteInterceptor());
        }

        private static void RegisterGenericRepositories()
        {
            var dbContextTypes =
                _typeFinder.Find(type =>
                    type.IsPublic &&
                    !type.IsAbstract &&
                    type.IsClass &&
                    typeof(AbpDbContext).IsAssignableFrom(type)
                    );

            if (dbContextTypes.IsNullOrEmpty())
            {
                Logger.Warn("No class found derived from AbpDbContext.");
                return;
            }

            foreach (var dbContextType in dbContextTypes)
            {
                EntityFrameworkGenericRepositoryRegistrar.RegisterForDbContext(dbContextType, IocManager);
            }
        }
    }
}
