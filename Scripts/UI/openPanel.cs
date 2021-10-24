using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class openPanel : MonoBehaviour
{
    [SerializeField] private GameObject levelOnePanel;
    [SerializeField] private GameObject selectPanel;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        // levelOnePanel = GameObject.Find("Level One Panel");
        // selectPanel = GameObject.Find("Level Select Panel");
        selectPanel.gameObject.SetActive(false);
        levelOnePanel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                selectPanel.gameObject.SetActive(!selectPanel.gameObject.activeSelf);
            }
        }

        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                levelOnePanel.gameObject.SetActive(!levelOnePanel.gameObject.activeSelf);
            }
        }
    }

    public void ReturnToMainScene() {
        selectPanel.gameObject.SetActive(false);
        levelOnePanel.gameObject.SetActive(false);
        SceneManager.LoadScene("Fake Main Scene");
    }
}
