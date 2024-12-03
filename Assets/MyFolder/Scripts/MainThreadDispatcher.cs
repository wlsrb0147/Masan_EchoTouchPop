using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> ActionQueue = new();

    public static void RunOnMainThread(Action action)
    {
        lock (ActionQueue)
        {
            ActionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (ActionQueue)
        {
            while (ActionQueue.Count > 0)
            {
                ActionQueue.Dequeue()?.Invoke();
            }
        }
    }
}