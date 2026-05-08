using UnityEngine;

namespace TowerDefense.Core
{
    public class LevelResult
    {
        [Tooltip("Số sao đạt được khi kết thúc màn (0-3).")]
        public int starCount;

        [Tooltip("Số mạng còn lại khi kết thúc màn.")]
        public int livesLeft;

        [Tooltip("Số wave đã hoàn thành trong màn hiện tại.")]
        public int wavesCleared;

        [Tooltip("Tên level đang chơi.")]
        public string levelName;

        [Tooltip("Kết quả trận đấu: true = thắng, false = thua.")]
        public bool isVictory;

        public LevelResult(string levelName, int livesLeft, int wavesCleared)
        {
            this.levelName = levelName;
            this.livesLeft = livesLeft;
            this.wavesCleared = wavesCleared;
            isVictory = livesLeft > 0;
            starCount = CalculateStars(livesLeft);
        }

        private int CalculateStars(int lives)
        {
            if (lives > 15) return 3;
            if (lives > 10) return 2;
            if (lives > 0) return 1;
            return 0;
        }

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
