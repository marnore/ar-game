using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Start minigame automatically </summary>*/
public class StartMinigame : MonoBehaviour
{
    [SerializeField] private MiniGame minigame;

    private void Start()
    {
        minigame.StartMinigame();
    }
}
