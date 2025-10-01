using UnityEngine;
using Wastelands.Core.Management;

namespace Wastelands.UI.Bootstrap
{
    /// <summary>
    /// MonoBehaviour placed in the Boot scene to install deterministic services at startup.
    /// </summary>
    public sealed class DeterministicBootstrapper : MonoBehaviour
    {
        [SerializeField] private DeterministicServiceInstaller? installer;

        private void Awake()
        {
            if (installer == null)
            {
                Debug.LogError("DeterministicBootstrapper missing installer reference.");
                return;
            }

            installer.Install();
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DeterministicServicesProvider.Clear();
        }
    }
}
