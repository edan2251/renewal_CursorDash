using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class LaserArea : MonoBehaviour
{
    public static readonly Color HalfColor = new Color(0.8f, 0f, 0f, 0.5f);
    public static readonly Color FullColor = new Color(0.8f, 0f, 0f, 1f);

    [SerializeField] private Collider2D area;
    [FormerlySerializedAs("renderer")]
    [SerializeField] private SpriteRenderer areaRenderer;
    private readonly List<Collider2D> colliders = new List<Collider2D>();

    public SpriteRenderer Renderer => areaRenderer;
    public bool IsPlayerWithin => area.OverlapPoint(PlayerCharacterController.instance.transform.position);
}
