using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ReloadThisLevel();
    }

    void ReloadThisLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
