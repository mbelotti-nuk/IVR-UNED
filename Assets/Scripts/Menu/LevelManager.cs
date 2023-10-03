using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject _loaderCanvas;
    [SerializeField] private Scrollbar _scrollbar;
    private float _target;
    public static LevelManager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void LoadScene(string sceneName)
    {
        _target = 0;
        _scrollbar.size = 0;
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        _loaderCanvas.SetActive(true);

        do
        {
            await System.Threading.Tasks.Task.Delay(100);
            _target = scene.progress;
            
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        //_loaderCanvas.SetActive(false);
    }

    private void Update()
    {
        _scrollbar.size = Mathf.MoveTowards(_scrollbar.size, _target, 0.7f * Time.deltaTime);
        if(_scrollbar.size > 0.8)
        {
            _loaderCanvas.SetActive(false);
        }
    }

}
