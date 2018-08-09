using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Interface for MiniGames </summary>*/
public interface IMiniGame
{
    string NoEquipmentMessage { get; }

    /**<summary> Start the minigame </summary>*/
    void StartMinigame();

    /**<summary> Stop the minigame </summary>*/
    void StopMinigame();
}
