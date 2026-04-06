using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public class LocationProgressBarViewItem : MonoBehaviour
    {
        [SerializeField]
        private Image _image;
        
        [SerializeField]
        private TMP_Text _text;

        public void Init(Sprite icon, int level)
        {
            _image.sprite = icon;
            _text.text = level.ToString();
        }
        
    }
}