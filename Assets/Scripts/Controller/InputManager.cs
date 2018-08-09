using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            Back();
    }

    /**<summary> Back / Esc key function </summary>*/
    public void Back()
    {
        switch (StateManager.sceneState)
        {
            case SceneState.Default:
                Application.Quit();
                break;
            case SceneState.Minigame:
                FindObjectOfType<MiniGame>().StopMinigame();
                break;
        }
    }
}
