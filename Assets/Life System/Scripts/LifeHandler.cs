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
        [Header("Max no of life")]
        [SerializeField] private int MaxLifeCount;
        [Header("Time in seconds")]
        [SerializeField] private int TimeToAddLifeInSeconds;
        
        private readonly string LifeDataKey = "LIFE";
        private LifeData lifeData = new LifeData();
        
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
            if (PlayerPrefs.HasKey(this.LifeDataKey))
            {
                lifeData = JsonUtility.FromJson<LifeData>(PlayerPrefs.GetString(this.LifeDataKey));
            }
            else
            {
                lifeData.CurrentLifeCount = MaxLifeCount;
                PlayerPrefs.SetString(this.LifeDataKey, JsonUtility.ToJson(lifeData));
            }
            CheckLife();
        }

        public void LooseLife()
        {
            if (lifeData.CurrentLifeCount > 0)
            {
                lifeData.CurrentLifeCount -= 1;
                PlayerPrefs.SetString(this.LifeDataKey, JsonUtility.ToJson(lifeData));
                SetTimeToAddNextLife();
            }
        }
        
        public void AddLife()
        {
            if (lifeData.CurrentLifeCount < MaxLifeCount)
            {
                lifeData.CurrentLifeCount += 1;
                PlayerPrefs.SetString(this.LifeDataKey, JsonUtility.ToJson(lifeData));
            }
        }
        
        public void RefillLife()
        {
            lifeData.CurrentLifeCount = MaxLifeCount;
            lifeData.AddedNextTime = new System.Collections.Generic.List<string>();
            PlayerPrefs.SetString(this.LifeDataKey, JsonUtility.ToJson(lifeData));
        }
        
        private void Update()
        {
            if (lifeData.CurrentLifeCount < MaxLifeCount)
            {
                if (lifeData.AddedNextTime.Count > 0)
                {
                    TimeSpan span = DateTime.Parse(lifeData.AddedNextTime[0]) - DateTime.Now;
                    LifeEvents.OnGetLifeDetail?.Invoke(lifeData.CurrentLifeCount, GetRemainingTime(span));
                    if (span.TotalSeconds < 0)
                    {
                        lifeData.AddedNextTime.RemoveAt(0);
                        AddLife();
                    }
                }
            }
            else
            {
                LifeEvents.OnGetLifeDetail?.Invoke(lifeData.CurrentLifeCount, "Full");
            }
        }
        
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
        
        private bool IsLifeIsFull()
        {
            return lifeData.CurrentLifeCount >= MaxLifeCount;
        }
        
        public bool CanPlay()
        {
            return lifeData.CurrentLifeCount > 0;
        }
        
        void SetTimeToAddNextLife()
        {
            var seconds = TimeToAddLifeInSeconds;
            if (lifeData.AddedNextTime.Count > 0)
            {
                string times = lifeData.AddedNextTime[lifeData.AddedNextTime.Count - 1];
                DateTime nextTime = DateTime.Parse(times).AddSeconds(seconds);
                lifeData.AddedNextTime.Add(nextTime.ToString());
            }
            else
            {
                DateTime nextTime = DateTime.Now.AddSeconds(seconds);
                lifeData.AddedNextTime.Add(nextTime.ToString());
            }
            PlayerPrefs.SetString(this.LifeDataKey, JsonUtility.ToJson(lifeData));
        }
        
        void CheckLife()
        {
            if (lifeData.AddedNextTime.Count > 0)
            {
                string times = lifeData.AddedNextTime[lifeData.AddedNextTime.Count - 1];
                TimeSpan span = DateTime.Parse(times) - DateTime.Now;
                LifeEvents.OnGetLifeDetail?.Invoke(lifeData.CurrentLifeCount, GetRemainingTime(span));
            }
        }
    }
}