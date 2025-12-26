using UnityEngine;

namespace EasyUI.PickerWheelUI
{
    [System.Serializable]
    public class WheelPiece
    {
        public Sprite icon;
        public string label;

        [Tooltip("Reward amount")] [SerializeField]
        private int amount;

        [Tooltip("Probability in %")] [Range(0f, 100f)] [SerializeField]
        private float chance = 100f;

        public int Index { get; set; }
        public double Weight { get; set; }
        public int Amount => this.amount;
        public float Chance => this.chance;
    }
}