using System;
using UnityEngine.Events;

namespace EventTools.Event
{
    [Serializable]
    public class UniEvent : UnityEvent { }

    [Serializable]
    public class UniEvent<T> : UnityEvent<T> { }
}
