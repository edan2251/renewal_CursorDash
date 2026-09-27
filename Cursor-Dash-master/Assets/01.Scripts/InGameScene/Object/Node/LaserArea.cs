using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserArea : MonoBehaviour
{
    public static readonly Color HalfColor = new Color(0.8f, 0f, 0f, 0.5f);
    public static readonly Color FullColor = new Color(0.8f, 0f, 0f, 1f);

    [SerializeField] private Collider2D area;
    [SerializeField] private new SpriteRenderer renderer;
    private readonly List<Collider2D> colliders = new List<Collider2D>();

    public SpriteRenderer Renderer => renderer;
    public bool IsPlayerWithin => area.OverlapPoint(PlayerCharacterController.instance.transform.position);
}