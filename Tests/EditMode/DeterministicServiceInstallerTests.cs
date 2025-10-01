using NUnit.Framework;
using UnityEngine;
using Wastelands.Core.Management;
using Wastelands.UI.Bootstrap;

namespace Wastelands.Tests.EditMode
{
    public class DeterministicServiceInstallerTests
    {
        [Test]
        public void Install_RegistersContainerWithProvider()
        {
            var installer = ScriptableObject.CreateInstance<DeterministicServiceInstaller>();

            try
            {
                var container = installer.Install();

                Assert.IsNotNull(container.TimeProvider);
                Assert.IsNotNull(DeterministicServicesProvider.Container);
            }
            finally
            {
                installer.ResetContainer();
                ScriptableObject.DestroyImmediate(installer);
            }
        }

        [Test]
        public void ConfigureSeed_UpdatesUnderlyingRng()
        {
            var installer = ScriptableObject.CreateInstance<DeterministicServiceInstaller>();

            try
            {
                var container = installer.Install();
                var original = container.RngService.Seed;

                installer.ConfigureSeed(original + 1);

                Assert.AreEqual(original + 1, container.RngService.Seed);
            }
            finally
            {
                installer.ResetContainer();
                ScriptableObject.DestroyImmediate(installer);
            }
        }
    }
}
