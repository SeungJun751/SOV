using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;


public class LoadingManager : MonoBehaviour
{

    public GameObject loadingScreen;
    private LoadingScreenManager loadingAnimator;

    private string sceneToLoad;

    void Start()
    {
        loadingAnimator = loadingScreen.GetComponent<LoadingScreenManager>();
        if (loadingAnimator != null)
        {
            loadingAnimator.SetOwner(this); 
        }
    }

    public void LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            loadingAnimator.RevealLoadingScreen(); 
        }
    }

    public void OnLoadingScreenRevealed()
    {
        StartCoroutine(LoadSceneAsync());
    }


    private IEnumerator LoadSceneAsync()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(sceneToLoad);
    }
}

