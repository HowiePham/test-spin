using System.Collections.Generic;

//Classes to Stote current life progress and time to add next life
namespace TechJuego.LifeSystem
{
    [System.Serializable]
    public class AddTimeClass
    {
        public int CurrentLifeCount;
        public List<string> AddedNextTime = new List<string>();
    }
    [System.Serializable]
    public class LifeData
    {
        public AddTimeClass lifedata = new AddTimeClass();
    }

}