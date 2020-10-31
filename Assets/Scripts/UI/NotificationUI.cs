using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Undercooked.UI
{
    public class NotificationUI : MonoBehaviour
    {
        private void Awake()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private static TextMeshProUGUI _text;

        public static async Task DisplayNotificationAsync(string textToDisplay, Color outlineColor, float timeToDisplayInSeconds = 2f)
        {
            _text.text = textToDisplay;
            _text.outlineColor = outlineColor;
            await Task.Delay((int)(timeToDisplayInSeconds * 1000f));
            _text.text = string.Empty;
        }
    }
    
}

