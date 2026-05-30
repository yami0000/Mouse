using UnityEngine;

public class SingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindAnyObjectByType(typeof(T));

                if (_instance == null)
                {
                    Debug.LogWarning($"{typeof(T).Name} not found. It's either been destroyed or hasn't been created.");
                }
            }
            return _instance;
        }
    }
}

