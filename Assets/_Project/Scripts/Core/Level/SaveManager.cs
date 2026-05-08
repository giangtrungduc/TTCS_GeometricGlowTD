using UnityEngine;

namespace TowerDefense.Core
{
    public static class SaveManager
    {
        private const string STAR_KEY = "LEVEL_{0}_STARS";
        private const string MUSIC_VOLUME_KEY = "MUSIC_VOLUME";
        private const string SFX_VOLUME_KEY   = "SFX_VOLUME";

        public static void SaveStars(int levelID, int stars)
        {
            string key = string.Format(STAR_KEY, levelID);

            int currentStars = PlayerPrefs.GetInt(key, 0);

            if (stars > currentStars)
            {
                PlayerPrefs.SetInt(key, stars);
                PlayerPrefs.Save();
            }
        }
        public static int GetStars(int levelID)
        {
            string key = string.Format(STAR_KEY, levelID);
            return PlayerPrefs.GetInt(key, 0);
        }
        public static bool IsLevelUnlocked(int levelID)
        {
            if (levelID == 0) return true;

            int previousLevelStars = GetStars(levelID - 1);
            return previousLevelStars > 0;
        }
        public static void SaveMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
            PlayerPrefs.Save();
        }

        public static float LoadMusicVolume()
        {
            return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        }

        public static void SaveSFXVolume(float volume)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
            PlayerPrefs.Save();
        }

        public static float LoadSFXVolume()
        {
            return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        }
        public static void ClearSaves()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}