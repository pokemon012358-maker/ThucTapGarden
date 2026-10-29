using System;
using UnityEngine;

namespace BalconyGarden
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Cấu hình thời gian")]
        [Tooltip("Giới hạn tối đa tính thời gian offline (giờ)")]
        public float maxOfflineHours = 12f;

        [Tooltip("Hệ số tăng tốc thời gian để test (1 = bình thường, 10 = nhanh gấp 10 lần)")]
        public float debugTimeScale = 1.0f;

        public event Action<float> OnOfflineTimeCalculated;

        private DateTime lastQuitTime;
        private float offlineSecondsElapsed = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Tính toán số giây trôi qua khi người chơi không mở game
        /// </summary>
        public float CalculateOfflineSeconds(string lastSaveTimeUtcString)
        {
            if (string.IsNullOrEmpty(lastSaveTimeUtcString))
            {
                return 0f;
            }

            if (DateTime.TryParse(lastSaveTimeUtcString, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastTime))
            {
                DateTime nowUtc = DateTime.UtcNow;
                if (nowUtc > lastTime)
                {
                    TimeSpan span = nowUtc - lastTime;
                    double totalSeconds = span.TotalSeconds;

                    // Giới hạn tối đa để tránh cheat đổi giờ máy quá đà
                    double maxSeconds = maxOfflineHours * 3600.0;
                    if (totalSeconds > maxSeconds)
                    {
                        totalSeconds = maxSeconds;
                    }

                    offlineSecondsElapsed = (float)totalSeconds;
                    Debug.Log($"[TimeManager] Offline time: {span.TotalMinutes:F1} minutes (Calculated: {offlineSecondsElapsed:F0}s)");
                    OnOfflineTimeCalculated?.Invoke(offlineSecondsElapsed);
                    return offlineSecondsElapsed;
                }
            }

            return 0f;
        }

        public float GetDeltaTime()
        {
            return Time.deltaTime * Mathf.Max(0.1f, debugTimeScale);
        }
    }
}
