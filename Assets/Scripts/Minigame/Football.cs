using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/**<summary> Football Minigame </summary>*/
public class Football : MiniGame, IMiniGame
{
    [SerializeField] private string noEquipmentMessage;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float reloadTime = 0.5f;
    [SerializeField] private float minDistance;

    [SerializeField] private GameObject goalKeeper;

    private MinigameItem currentBall;
    private float originalScale;
    private int shots, goals, score;

    /**<summary> Message to show if required equipment is not available </summary>*/
    override public string NoEquipmentMessage
    {
        get { return noEquipmentMessage; }
    }

    private void Start()
    {
        originalScale = transform.localScale.magnitude;
    }

    /**<summary> Reset state of the minigame (score etc.) </summary>*/
    public void ResetState()
    {
        shots = 0; goals = 0; score = 0;
        OnScoreChanged?.Invoke(score);
    }

    /**<summary> Start the minigame </summary>*/
    override public void StartMinigame()
    {
        if (AvailableEquipment <= 0)
        {
            DialogManager.ShowAlert("Equipment requirements",
                noEquipmentMessage,
                true,
                new DialogManager.DialogButton("Ok", () => { })
            );
            return;
        }
        ResetState();
        transform.parent = null;
        if (FindObjectOfType<WorldController>())
            FindObjectOfType<WorldController>().Pause();

        StateManager.sceneState = SceneState.Minigame;

        Vector3 lookPos = transform.position - player.transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.DOMove(transform.position + (lookPos).normalized * (minDistance - lookPos.magnitude + minDistance*0.1f), 0.8f);
        transform.DORotateQuaternion(rotation, 0.8f);
        transform.DOScale(1f, 0.8f);

        GetComponent<HitControls>().enabled = true;
        goalKeeper.SetActive(true);
        active = true;
        SetBall();
    }

    /**<summary> Start the minigame </summary>*/
    override public void StopMinigame()
    {
        active = false;
        if (currentBall)
            Destroy(currentBall.gameObject);
        if (FindObjectOfType<WorldController>())
        {
            transform.parent = FindObjectOfType<WorldController>().itemRoot;
            FindObjectOfType<WorldController>().Resume();
        }

        transform.DOMove(data.position, 0.8f);
        transform.DORotateQuaternion(data.rotation, 0.8f);
        transform.DOScale(originalScale, 0.8f);

        GetComponent<HitControls>().enabled = false;
        goalKeeper.SetActive(false);
        StateManager.sceneState = SceneState.Default;

        float goalAccuracy = (goals / (float)(shots+0.1f));
        int exp = (int)(score * 0.5f);
        int credits = (int)(score * 0.2f);

        player.experience += exp;
        player.credits += credits;
#pragma warning disable 4014
        player.Save();
#pragma warning restore 4014
        DialogManager.ShowAlert("Football Results",
            $"Total score: {score}\nGoals: {goals}/{shots} (Accuracy: {(goalAccuracy*100).ToString("f0")}%)\nReward: {exp} Exp and {credits} Credits",
            true,
            new DialogManager.DialogButton("Ok", () => { })
        );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            StartMinigame();
        if (active && currentBall && currentBall.moved)
        {
            shots++;
            ConsumeEquipment();
            OnItemCountChanged?.Invoke(AvailableEquipment);
            currentBall = null;
            Invoke("SetBall", 1);
        }
    }

    /**<summary> Set new ball to start position </summary>*/
    public void SetBall()
    {
        if (AvailableEquipment > 0)
        {
            Transform ball = Instantiate(ballPrefab, player.transform).transform;
            Vector3 position = ball.localPosition;
            ball.localPosition = new Vector3(0, 0, -1);
            currentBall = ball.GetComponent<MinigameItem>();
            ball.DOLocalMove(position, reloadTime);
        }
        else
            OutOfEquipment();
    }

    private void OnTriggerEnter(Collider other)
    {
        MinigameItem item = other.GetComponent<MinigameItem>();
        if (item.scored)
            return;
        goals++;
        score += (int)((transform.position - player.transform.position).magnitude * 2 + (transform.position - other.transform.position).magnitude * 10);
        OnScoreChanged?.Invoke(score);
        item.scored = true;
    }
}
