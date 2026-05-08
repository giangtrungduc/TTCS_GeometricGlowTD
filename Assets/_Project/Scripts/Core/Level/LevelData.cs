using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TowerDefense.Core
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "TD/LevelData", order = 5)]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Ma so cua level, dung de luu tru progression.")]
        public int levelID;

        [Tooltip("Ten hien thi cua level tren UI.")]
        public string levelName;

        [Tooltip("Ten scene runtime de load level.")]
        [SerializeField] private string sceneName;

        [Tooltip("Path scene runtime trong Build Settings. Neu co se duoc uu tien.")]
        [SerializeField, HideInInspector] private string scenePath;

        [Tooltip("Bieu tuong cua level.")]
        public Sprite iconLevel;

        [Tooltip("Hinh nen cua level.")]
        public Sprite backgroundLevel;

#if UNITY_EDITOR
        [Header("Editor Scene Reference")]
        [SerializeField] private SceneAsset sceneAsset;
#endif

        public string SceneName => sceneName;
        public string ScenePath => scenePath;

        public bool TryGetSceneIdentifier(out string sceneIdentifier)
        {
            sceneIdentifier = !string.IsNullOrWhiteSpace(scenePath)
                ? scenePath
                : sceneName;

            return !string.IsNullOrWhiteSpace(sceneIdentifier);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(levelName))
            {
                levelName = name;
            }

            if (sceneAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
                scenePath = assetPath;
                sceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            }

            if (!TryGetSceneIdentifier(out string sceneIdentifier))
            {
                Debug.LogWarning($"[LevelData] '{name}' chua gan scene runtime.", this);
                return;
            }

            bool isInBuild = false;
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled) continue;

                string buildScenePath = scenes[i].path;
                string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(buildScenePath);

                if (buildScenePath == sceneIdentifier || buildSceneName == sceneIdentifier)
                {
                    isInBuild = true;
                    break;
                }
            }

            if (!isInBuild)
            {
                Debug.LogWarning($"[LevelData] '{name}' tro toi scene chua co trong Build Settings: {sceneIdentifier}", this);
            }
        }
#endif
    }

    public static class SceneLoader
    {
        public const string MainMenuScene = "MainMenu";
        public const string LevelSelectScene = "LevelSelected";

        public static bool TryLoadScene(string sceneIdentifier, Object context = null)
        {
            if (!TryResolveScene(sceneIdentifier, out string resolvedScene))
            {
                Debug.LogError($"[SceneLoader] Scene khong hop le hoac chua co trong Build Settings: {sceneIdentifier}", context);
                return false;
            }

            SceneManager.LoadScene(resolvedScene);
            return true;
        }

        public static bool TryLoadLevel(LevelData levelData, Object context = null)
        {
            if (levelData == null)
            {
                Debug.LogError("[SceneLoader] LevelData bi null.", context);
                return false;
            }

            if (!levelData.TryGetSceneIdentifier(out string sceneIdentifier))
            {
                Debug.LogError($"[SceneLoader] Level '{levelData.name}' chua duoc cau hinh scene.", context ?? levelData);
                return false;
            }

            return TryLoadScene(sceneIdentifier, context ?? levelData);
        }

        public static bool CanLoadScene(string sceneIdentifier)
        {
            return TryResolveScene(sceneIdentifier, out _);
        }

        private static bool TryResolveScene(string sceneIdentifier, out string resolvedScene)
        {
            resolvedScene = string.Empty;

            if (string.IsNullOrWhiteSpace(sceneIdentifier))
            {
                return false;
            }

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string buildScenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrWhiteSpace(buildScenePath))
                {
                    continue;
                }

                string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(buildScenePath);

                if (buildScenePath == sceneIdentifier)
                {
                    resolvedScene = buildScenePath;
                    return true;
                }

                if (string.Equals(buildSceneName, sceneIdentifier, System.StringComparison.Ordinal))
                {
                    resolvedScene = buildSceneName;
                    return true;
                }
            }

            return false;
        }
    }
}
