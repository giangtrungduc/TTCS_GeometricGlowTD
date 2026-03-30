using UnityEngine;

namespace TowerDefense.Core
{
    public class LevelResult
    {
        public int starCount; // Số sao đạt được
        public int livesLeft; // Số mạng còn lại khi kết thúc màn
        public int wavesCleared; // Số wave đã hoàn thành
        public string levelName; // Level đang chơi

        /// <summary>
        /// Kết quả là thắng hay thua.
        /// true = Win (còn lives > 0 khi hết wave).
        /// false = Lose (lives = 0).
        /// </summary>
        public bool isVictory;

        // ============================
        // CONSTRUCTOR
        // ============================
        public LevelResult(string levelName, int livesLeft, int wavesCleared)
        {
            this.levelName = levelName;
            this.livesLeft = livesLeft;
            this.wavesCleared = wavesCleared;

            this.isVictory = livesLeft > 0;

            this.starCount = CalculateStars(livesLeft);
        }

        // ============================
        // PRIVATE METHODS
        // ============================
        private int CalculateStars(int lives)
        {
            if (lives > 15) return 3;
            else if (lives > 10) return 2;
            else if (lives > 0) return 1;
            else return 0;
        }

        // ============================
        // UTILITY
        // ============================
        public override string ToString()
        {
            string result = isVictory ? "VICTORY" : "DEFEAT";
            string stars = new string('★', starCount)
                         + new string('☆', 3 - starCount);

            return $"[{result}] {levelName} | {stars} | "
                 + $"Lives: {livesLeft} | Waves: {wavesCleared}";
        }
    }
}
