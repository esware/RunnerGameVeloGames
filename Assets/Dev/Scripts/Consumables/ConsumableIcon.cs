using UnityEngine;
using UnityEngine.UI;

namespace Dev.Scripts.Consumables
{
    public class ConsumableIcon : MonoBehaviour
    {
        public Text numberTxt;
        public Image iconImg;
        public Consumable _consumable;
        private Button btn;
        private CharacterControl _characterControl;
        private void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(TaskOnClick);
            _characterControl = FindObjectOfType<CharacterControl>();
        }

        private void TaskOnClick()
        {
            _characterControl.UseInventory(this);
        }
    }
}