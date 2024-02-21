using Dev.Scripts.Consumables;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.Scripts.UI
{
    public class PowerupIcon : MonoBehaviour
    {
        [HideInInspector]
        public Consumable linkedConsumable;

        public Image icon;
        public Slider slider;
        public bool isStart=false;
    
        void Update()
        {
            if (isStart)
            {
                slider.gameObject.SetActive(true);
                icon.sprite = linkedConsumable.icon;
                slider.value = 1.0f - linkedConsumable.timeActive / linkedConsumable.duration;
            }
        }
    }
}