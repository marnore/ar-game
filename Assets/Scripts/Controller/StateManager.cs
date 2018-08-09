using System;

public enum SceneState { Default, Minigame };
/**<summary> Store current state of the game to check for controls etc. </summary>*/
public static class StateManager
{
    private static SceneState _sceneState = SceneState.Default;
    public static SceneState sceneState
    {
        get { return _sceneState; }
        set
        {
            _sceneState = value;
            OnSceneStateChanged?.Invoke(_sceneState);
        }
    }

    /**<summary> Callback function when SceneState is changed </summary>*/
    public static Action<SceneState> OnSceneStateChanged;
}
