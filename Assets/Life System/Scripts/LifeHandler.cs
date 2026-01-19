using System;
using UnityEngine;
namespace TechJuego.LifeSystem
{
    //Events to get update of current life count and remaining time
    public class LifeEvents
    {
        public delegate void OnUpdateLife(int lifecount, string remainingTime);
        public static OnUpdateLife OnGetLifeDetail;
    }
    public class LifeHandler : MonoBehaviour
    {
        public static LifeHandler Instance;
        private readonly string lifeHolder = "LIFE";
        private LifeData lifeData = new LifeData();
        [Header("Max no of life")]
        [SerializeField] private int MaxLifeCount;
        [Header("Time in seconds")]
        [SerializeField] private int TimeToAddLifeInSeconds;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            if (PlayerPrefs.HasKey(lifeHolder))
            {
                lifeData = JsonUtility.FromJson<LifeData>(PlayerPrefs.GetString(lifeHolder));
            }
            else
            {
                lifeData.lifedata.CurrentLifeCount = MaxLifeCount;
                PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
            }
            CheckLife();
        }
        //function to reduce life count and check it don't go below 0
        public void LooseLife()
        {
            if (lifeData.lifedata.CurrentLifeCount > 0)
            {
                lifeData.lifedata.CurrentLifeCount -= 1;
                PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
                SetTimeToAddNextLife();
            }
        }
        //function to add life count and check it don't go above max count
        public void AddLife()
        {
            if (lifeData.lifedata.CurrentLifeCount < MaxLifeCount)
            {
                lifeData.lifedata.CurrentLifeCount += 1;
                PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
            }
        }
        //Refill all lifes
        public void RefillLife()
        {
            lifeData.lifedata.CurrentLifeCount = MaxLifeCount;
            lifeData.lifedata.AddedNextTime = new System.Collections.Generic.List<string>();
            PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
        }
        // update remaining time
        private void Update()
        {
            if (lifeData.lifedata.CurrentLifeCount < MaxLifeCount)
            {
                if (lifeData.lifedata.AddedNextTime.Count > 0)
                {
                    TimeSpan span = DateTime.Parse(lifeData.lifedata.AddedNextTime[0]) - DateTime.Now;
                    LifeEvents.OnGetLifeDetail?.Invoke(lifeData.lifedata.CurrentLifeCount, GetRemainingTime(span));
                    if (span.TotalSeconds < 0)
                    {
                        lifeData.lifedata.AddedNextTime.RemoveAt(0);
                        AddLife();
                    }
                }
            }
            else
            {
                LifeEvents.OnGetLifeDetail?.Invoke(lifeData.lifedata.CurrentLifeCount, "Full");
            }
        }
        // get time in string format to show
        private string GetRemainingTime(TimeSpan timeSpan)
        {
            string time = "";
            if (timeSpan.TotalSeconds <= 0)
            {
                time = "Full";
            }
            else
            {
                time = String.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
            }
            return time;
        }
        //function to check is if we have max life
        private bool IsLifeIsFull()
        {
            return lifeData.lifedata.CurrentLifeCount >= MaxLifeCount;
        }
        //function to check before can we play
        public bool CanPlay()
        {
            return lifeData.lifedata.CurrentLifeCount > 0;
        }
        //function to set time to add next life
        void SetTimeToAddNextLife()
        {
            var seconds = TimeToAddLifeInSeconds;
            if (lifeData.lifedata.AddedNextTime.Count > 0)
            {
                string times = lifeData.lifedata.AddedNextTime[lifeData.lifedata.AddedNextTime.Count - 1];
                DateTime nextTime = DateTime.Parse(times).AddSeconds(seconds);
                lifeData.lifedata.AddedNextTime.Add(nextTime.ToString());
            }
            else
            {
                DateTime nextTime = DateTime.Now.AddSeconds(seconds);
                lifeData.lifedata.AddedNextTime.Add(nextTime.ToString());
            }
            PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
        }
        //function to check on start 
        void CheckLife()
        {
            if (lifeData.lifedata.AddedNextTime.Count > 0)
            {
                string times = lifeData.lifedata.AddedNextTime[lifeData.lifedata.AddedNextTime.Count - 1];
                TimeSpan span = DateTime.Parse(times) - DateTime.Now;
                LifeEvents.OnGetLifeDetail?.Invoke(lifeData.lifedata.CurrentLifeCount, GetRemainingTime(span));
            }
        }
    }
}