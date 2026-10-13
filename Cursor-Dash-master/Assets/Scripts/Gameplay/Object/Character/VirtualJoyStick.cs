using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>A floating stick owned by the finger that began the gesture.</summary>
public class VirtualJoyStick : MonoBehaviour
{
    [Header("Initial feel settings (tune on devices)")]
    [SerializeField, Range(0.04f, 0.25f)] private float screenRadius = 0.12f;
    [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.12f;

    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private PointerEventData pointerData;
    private EventSystem pointerEventSystem;
    private SpriteRenderer frame;
    private SpriteRenderer knob;
    private Camera inputCamera;
    private Vector2 origin;
    private int fingerId = -1;
    private bool mouseGesture;
#if UNITY_EDITOR || UNITY_STANDALONE
    private bool keyboardReady = true;
#endif
    private Vector2Int screenSize;

    public bool HasGesture => fingerId >= 0 || mouseGesture;
    private float Radius => Mathf.Max(24f, Mathf.Min(Screen.width, Screen.height) * screenRadius);
    private bool CanMove => StageManager.instance != null && StageManager.instance.IsPlaying;

    private void Awake()
    {
        frame = GetComponent<SpriteRenderer>();
        knob = transform.childCount > 0 ? transform.GetChild(0).GetComponent<SpriteRenderer>() : null;
        inputCamera = Camera.main;
        screenSize = new Vector2Int(Screen.width, Screen.height);
        SetVisible(false);
    }

    private void Update()
    {
        Vector2Int currentSize = new Vector2Int(Screen.width, Screen.height);
        if (screenSize != currentSize)
        {
            screenSize = currentSize;
            ResetInput();
        }
        if (!CanMove)
        {
            ResetInput();
            return;
        }

        if (Input.touchCount > 0 || fingerId >= 0)
            ReadTouch();
        else
            ReadMouse();

#if UNITY_EDITOR || UNITY_STANDALONE
        // A fresh vector each frame prevents the old accumulating/stuck WASD input.
        if (!HasGesture)
        {
            Vector2 keyboard = new Vector2(
                (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
                (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));
            // After an interruption, a held key must be released before steering resumes.
            if (!keyboardReady)
            {
                if (keyboard == Vector2.zero) keyboardReady = true;
                keyboard = Vector2.zero;
            }
            SetMovement(Vector2.ClampMagnitude(keyboard, 1f));
        }
#endif
    }

    private void ReadTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
                TryBeginPointer(touch.fingerId, touch.position, IsOverButton(touch.position));

            if (touch.fingerId != fingerId) continue;
            if (touch.phase == TouchPhase.Canceled)
                CancelPointer(touch.fingerId);
            else if (touch.phase == TouchPhase.Ended)
                ReleasePointer(touch.fingerId);
            else
                ApplyPosition(touch.position);
            return;
        }

        // The browser may remove a canceled finger without sending an Ended frame.
        if (fingerId >= 0) ResetInput();
    }

    private void ReadMouse()
    {
        Vector2 position = Input.mousePosition;
        if (Input.GetMouseButtonDown(0) && !IsOverButton(position))
        {
            origin = position;
            mouseGesture = true;
            SetVisible(true);
        }
        if (!mouseGesture) return;
        if (Input.GetMouseButton(0)) ApplyPosition(position);
        else ReleaseGesture();
    }

    public static Vector2 FollowOrigin(Vector2 currentOrigin, Vector2 pointer, float radius)
    {
        if (radius <= 0f) return pointer;
        Vector2 displacement = pointer - currentOrigin;
        return displacement.sqrMagnitude > radius * radius
            ? pointer - displacement.normalized * radius
            : currentOrigin;
    }

    public static Vector2 CalculateInput(Vector2 displacement, float radius, float deadZone)
    {
        if (radius <= 0f) return Vector2.zero;
        deadZone = Mathf.Clamp(deadZone, 0f, 0.95f);
        float magnitude = Mathf.Clamp01(displacement.magnitude / radius);
        if (magnitude <= deadZone) return Vector2.zero;
        return displacement.normalized * ((magnitude - deadZone) / (1f - deadZone));
    }

    public bool TryBeginPointer(int id, Vector2 position, bool startsOnUI)
    {
        if (id < 0 || startsOnUI || HasGesture) return false;
        fingerId = id;
        mouseGesture = false;
        origin = position;
        SetVisible(true);
        return true;
    }

    public void ReleasePointer(int id) { if (id == fingerId) ReleaseGesture(); }
    public void CancelPointer(int id) { if (id == fingerId) ResetInput(); }

    private void ApplyPosition(Vector2 position)
    {
        // Screen-space deltas stay stable while the gameplay camera shakes.
        origin = FollowOrigin(origin, position, Radius);
        Vector2 displacement = position - origin;
        SetMovement(CalculateInput(displacement, Radius, deadZone));
        if (inputCamera == null) return;

        Vector3 pivot = ScreenPoint(origin);
        Vector3 edge = ScreenPoint(origin + Vector2.right * Radius);
        transform.position = pivot;
        if (frame != null && frame.sprite != null)
        {
            float extent = Mathf.Max(0.001f, frame.sprite.bounds.extents.x);
            transform.localScale = Vector3.one * (Vector3.Distance(pivot, edge) / extent);
        }
        if (knob != null)
            knob.transform.position = ScreenPoint(origin + Vector2.ClampMagnitude(displacement, Radius));
    }

    private Vector3 ScreenPoint(Vector2 point)
    {
        Vector3 world = inputCamera.ScreenToWorldPoint(new Vector3(point.x, point.y, -inputCamera.transform.position.z));
        world.z = 0f;
        return world;
    }

    private bool IsOverButton(Vector2 point)
    {
        EventSystem current = EventSystem.current;
        if (current == null) return false;
        if (pointerData == null || pointerEventSystem != current)
        {
            pointerEventSystem = current;
            pointerData = new PointerEventData(current);
        }
        pointerData.position = point;
        uiHits.Clear();
        current.RaycastAll(pointerData, uiHits);
        foreach (RaycastResult hit in uiHits)
            if (hit.gameObject.GetComponentInParent<Selectable>() != null) return true;
        return false;
    }

    private void SetMovement(Vector2 value)
    {
        if (PlayerCharacterController.instance != null)
            PlayerCharacterController.instance.MoveVector = value;
    }

    public void ResetInput()
    {
        fingerId = -1;
        mouseGesture = false;
#if UNITY_EDITOR || UNITY_STANDALONE
        keyboardReady = false;
#endif
        PlayerCharacterController.instance?.MoveStop();
        SetVisible(false);
    }

    private void ReleaseGesture()
    {
        fingerId = -1;
        mouseGesture = false;
        SetMovement(Vector2.zero);
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (frame != null) frame.enabled = visible;
        if (knob != null) knob.enabled = visible;
    }

    private void OnDisable() => ResetInput();
    private void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
    private void OnApplicationPause(bool paused) { if (paused) ResetInput(); }
}
