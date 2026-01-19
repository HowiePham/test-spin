using System.Collections.Generic;

namespace TechJuego.LifeSystem
{
    [System.Serializable]
    public class LifeData
    {
        public int CurrentLifeCount;
        public List<string> AddedNextTime = new List<string>();
    }
}