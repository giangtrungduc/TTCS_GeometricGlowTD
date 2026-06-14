using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.UI
{
    /// <summary>
    /// Trình điều khiển chung cho màn hình LevelSelect.
    /// Chủ yếu chứa hàm để quay về MainMenu.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        public void BackToMainMenu()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
