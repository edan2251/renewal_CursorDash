using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T cachedInstance;

    public static T instance
    {
        get
        {
            if (cachedInstance == null)
                cachedInstance = FindFirstObjectByType<T>();

            return cachedInstance;
        }
    }
}

public class DontDestroySingleton<T> : Singleton<T> where T : MonoBehaviour
{
    private static T persistentInstance;

    public new static T instance
    {
        get
        {
            if (persistentInstance == null)
            {
                persistentInstance = Singleton<T>.instance;
                if (persistentInstance != null)
                    DontDestroyOnLoad(persistentInstance.gameObject);
            }

            return persistentInstance;
        }
    }
}
