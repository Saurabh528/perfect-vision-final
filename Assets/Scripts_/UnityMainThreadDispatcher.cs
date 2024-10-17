using System.Collections.Generic;
using UnityEngine;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;

    private readonly Queue<Action> actionQueue = new Queue<Action>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        while (actionQueue.Count > 0)
        {
            Action action = actionQueue.Dequeue();
            action.Invoke();
        }
    }

    public void Enqueue(Action action)
    {
        lock (actionQueue)
        {
            actionQueue.Enqueue(action);
        }
    }

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
        }

        return instance;
    }
}