using UnityEngine;

public class CloseGame : MonoBehaviour
{
    private const int NumberOfClick = 5;
    private int _currentNumberOfClick;
    private float _timer;
    private const float TimeLimit = 10;

    void Update()
    {
        if (_currentNumberOfClick == 0) return;

        _timer += Time.deltaTime;

        if (_timer >= TimeLimit)
        {
            _currentNumberOfClick = 0;
            _timer = 0;
        }

        if (_currentNumberOfClick >= NumberOfClick)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();       
#endif
        }
    }

    public void OnClick()
    {
        ++_currentNumberOfClick;
    }
}
