using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameUI : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text itemCountText;

    private void Awake()
    {
        StateManager.OnSceneStateChanged += OnSceneStateChanged;
    }

    public void OnSceneStateChanged(SceneState state)
    {
        if (state == SceneState.Minigame)
        {
            MiniGame minigame = FindObjectOfType<MiniGame>();
            minigame.OnScoreChanged += OnScoreChanged;
            minigame.OnItemCountChanged += OnItemCountChanged;
        }
    }

    private void OnItemCountChanged(int itemCount)
    {
        itemCountText.text = $"Items: {itemCount}";
    }

    private void OnScoreChanged(int score)
    {
        scoreText.text = $"Score: {score}";
    }
}
