using UnityEngine.SceneManagement;

namespace LD.Scenes
{
    public class SceneManaging
    {
        public static void LoadScene(SceneField[] sceneToLoad)
        {
            for (int i = 0; i < sceneToLoad.Length; i++)
            {
                bool isSceneLoaded = false;
                for (int j = 0; j < SceneManager.sceneCount; j++)
                {
                    Scene loadedScene = SceneManager.GetSceneAt(j);
                    if (loadedScene.name == sceneToLoad[i].SceneName)
                    {
                        isSceneLoaded = true;
                        break;
                    }
                }

                if (!isSceneLoaded)
                {
                    SceneManager.LoadSceneAsync(sceneToLoad[i], LoadSceneMode.Additive);
                }
            }
        }

        public static void UnloadScene(SceneField[] sceneToUnload)
        {
            for (int i = 0; i < sceneToUnload.Length; i++)
            {
                for (int j = 0; j < SceneManager.sceneCount; j++)
                {
                    Scene loadedScene = SceneManager.GetSceneAt(j);
                    if (loadedScene.name == sceneToUnload[i].SceneName)
                    {
                        SceneManager.UnloadSceneAsync(sceneToUnload[i]);
                    }
                }
            }
        }
    }
}