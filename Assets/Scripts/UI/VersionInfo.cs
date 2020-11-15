using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.UI
{
    public class VersionInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI versionInfo;

        private void Awake()
        {
#if UNITY_EDITOR
            Assert.IsNotNull(versionInfo);
#endif
            versionInfo.text = $"v {Application.version}";
        }
    }
}
