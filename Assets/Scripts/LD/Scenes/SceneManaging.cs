using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD.Scenes
{
    public class SceneManaging
    {
        public static void LoadScene(SceneField[] sceneToLoad)
        {
            if (sceneToLoad.Length == 0) return;
            // changement de lighting en mettant la scene active
            AsyncOperation firstOp = SceneManager.LoadSceneAsync(sceneToLoad[0], LoadSceneMode.Additive);
            firstOp.completed += _ =>
            {
                Scene newScene = SceneManager.GetSceneByName(sceneToLoad[0].SceneName);
                if (newScene.IsValid())
                    SceneManager.SetActiveScene(newScene);
            };

            // loading du reste
            for (int i = 1; i < sceneToLoad.Length; i++)
            {
                bool isSceneLoaded = false;
                for (int j = 0; j < SceneManager.sceneCount; j++)
                {
                    if (SceneManager.GetSceneAt(j).name == sceneToLoad[i].SceneName)
                    {
                        isSceneLoaded = true;
                        break;
                    }
                }

                if (!isSceneLoaded)
                    SceneManager.LoadSceneAsync(sceneToLoad[i], LoadSceneMode.Additive);
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