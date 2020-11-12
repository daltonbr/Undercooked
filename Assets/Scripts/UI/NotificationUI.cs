using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Undercooked.UI
{
    public class NotificationUI : MonoBehaviour
    {
        private static TextMeshProUGUI _text;
        
        private void Awake()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
            
            #if UNITY_EDITOR
                Assert.IsNotNull(_text);
            #endif
        }

        public static async Task DisplayCenterNotificationAsync(
            string textToDisplay, Color outlineColor, float timeToDisplayInSeconds = 2f)
        {
            _text.text = textToDisplay;
            _text.outlineColor = outlineColor;
            await Task.Delay((int)(timeToDisplayInSeconds * 1000f));
            _text.text = string.Empty;
        }
    }
    
}
