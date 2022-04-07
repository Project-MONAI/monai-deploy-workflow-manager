using BoDi;

namespace Monai.Deploy.WorkloadManager.IntegrationTests
{
    internal static class IocServiceLocator
    {
        internal static IObjectContainer Services => HooksContainer;

        private static IObjectContainer HooksContainer { get; set; }

        internal static void Init(IObjectContainer container) => HooksContainer = container;
    }

}
