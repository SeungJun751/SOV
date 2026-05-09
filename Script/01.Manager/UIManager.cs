using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<UIManager>();
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("UIPrefab_Canvas");
                    Instantiate(prefab);
                }
            }
            return _instance;
        }
    }

    public static bool IsUIOpen { get; private set; }

    public GameObject optionsWindow;

    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
    private GameObject currentOpenPanel;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.transform.parent.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.transform.parent.gameObject);

        RegisterPanels();
    }

    private void RegisterPanels()
    {
        Transform canvasTransform = this.transform.parent;
        foreach (Transform child in canvasTransform)
        {

            panels[child.name] = child.gameObject;
        }
    }

    public void OpenUI(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            if (currentOpenPanel != null)
            {
                currentOpenPanel.SetActive(false);
            }

            panels[panelName].SetActive(true);
            currentOpenPanel = panels[panelName];
            IsUIOpen = true;
        }
        else
        {
            Debug.LogWarning($"'{panelName}' 패널을 찾을 수 없습니다!");
        }
    }

    public void CloseCurrentUI()
    {
        if (currentOpenPanel != null)
        {
            currentOpenPanel.SetActive(false);
            currentOpenPanel = null;
            IsUIOpen = false;
        }
    }

    public GameObject GetPanel(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            return panels[panelName];
        }
        Debug.LogWarning($"'{panelName}' 패널을 찾을 수 없습니다!");
        return null;
    }

    public void OpenOptionsMenu()
    {
        if (optionsWindow != null)
        {
            optionsWindow.SetActive(true);
            Time.timeScale = 0f;
            IsUIOpen = true;
        }

    }

    public void CloseOptionsMenu()
    {
        if (optionsWindow != null)
        {
            optionsWindow.SetActive(false);
            Time.timeScale = 1f;
            IsUIOpen = false;
        }
    }
}