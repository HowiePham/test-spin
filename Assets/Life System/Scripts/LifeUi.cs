using System;
using UnityEngine;
using UnityEngine.UI;
namespace TechJuego.LifeSystem
{
    public class LifeUi : MonoBehaviour
    {
        public Text m_LifeCount;
        public Text m_TileRemaining;
        private void OnEnable()
        {
            LifeEvents.OnGetLifeDetail += LifeEvents_OnUpdateLife;
        }
        private void LifeEvents_OnUpdateLife(int lifecount, string remainingTime)
        {
            m_LifeCount.text = lifecount.ToString();
            m_TileRemaining.text = remainingTime;
        }
        private void OnDisable()
        {
            LifeEvents.OnGetLifeDetail -= LifeEvents_OnUpdateLife;
        }
    }
}