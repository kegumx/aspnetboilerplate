using Abp.Modules;
using System.Reflection;
using Abp.Reflection;
using Abp.Dependency;
using Castle.Core.Logging;

namespace Abp.AutoMapper
{
    internal static class AbpAutoMapperRegistrar
    {

        public static void Initialize(IIocManager iocManager)
        {
            var Logger = iocManager.IocContainer.Resolve<ILogger>();
            var _typeFinder = iocManager.IocContainer.Resolve<ITypeFinder>();
            var types = _typeFinder.Find(type =>
                type.IsDefined(typeof (AutoMapAttribute)) ||
                type.IsDefined(typeof (AutoMapFromAttribute)) ||
                type.IsDefined(typeof (AutoMapToAttribute))
                );

            Logger.DebugFormat("Found {0} classes defines auto mapping attributes", types.Length);
            foreach (var type in types)
            {
                Logger.Debug(type.FullName);
                AutoMapperHelper.CreateMap(type);
            }

        }
    }
}
