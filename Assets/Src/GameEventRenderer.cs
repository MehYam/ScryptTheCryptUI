﻿using System.Collections.Generic;
using UnityEngine;

using ScryptTheCrypt;

public class GameEventRenderer : MonoBehaviour
{
    [SerializeField] private GameObject playerParent = null;
    [SerializeField] private GameObject mobParent = null;

    private MainUILink ui;
    private MainAssetLink assets;
    void Start()
    {
        ui = GetComponent<MainUILink>();
        assets = GetComponent<MainAssetLink>();

        GameEvents.Instance.ActorAdded += (g, a) =>
        {
            EnqueueEvent(new ActorAddedRenderer(this, a));
        };
        GameEvents.Instance.ActorRemoved += (g, a) =>
        {
            EnqueueEvent(new ActorRemovedRenderer(a));
        };
        GameEvents.Instance.RoundStart += g =>
        {
            Debug.Log("start of turn");
            EnqueueEvent(new RoundStartRenderer(g.NumRounds));
        };
        GameEvents.Instance.ActorActionsStart += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} starts");
            EnqueueEvent(new ActorActionsStartRenderer(a));
        };
        GameEvents.Instance.ActorActionsEnd += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} ends");
            EnqueueEvent(new ActorActionsEndRenderer(a));
        };
        GameEvents.Instance.TargetSelected += a =>
        {
            Debug.Log($"{a.uniqueName} chooses target {a.Target}");
            EnqueueEvent(new TargetSelectedRenderer(a));
        };
        GameEvents.Instance.AttackStart += (a, b) =>
        {
            Debug.Log($"{a.uniqueName} attacks {b.uniqueName}");
            EnqueueEvent(new AttackRenderer(a, b));
        };
        GameEvents.Instance.ActorHealthChange += (a, oldHealth, newHealth) =>
        {
            Debug.Log($"{a.uniqueName} health {oldHealth} => {newHealth}");
            EnqueueEvent(new HealthChangeRenderer(a, oldHealth, newHealth));
        };
        GameEvents.Instance.Death += a =>
        {
            Debug.Log($"RIP {a.uniqueName}");
            EnqueueEvent(new DeathRenderer(a));
        };
    }
    private void OnDestroy()
    {
        GameEvents.ReleaseAllListeners();
    }
    readonly struct GameActorState
    {
        public const int NULL_ID = -1;

        public readonly int id;
        public readonly string name;
        public readonly string uniqueName;
        public readonly GameActor.Alignment align;
        public readonly float baseHealth;
        public readonly float health;
        public GameActorState(GameActor actor)
        {
            id = actor.id;
            name = actor.name;
            uniqueName = actor.uniqueName;
            align = actor.align;
            baseHealth = actor.baseHealth;
            health = actor.Health;
        }
    }
    static private void LayoutActors(GameObject actorParent, GameActor.Alignment alignment)
    {
        var slots = actorParent.GetComponentsInChildren<CharacterSlot>();

        var rect = Util.GetScreenRectInWorldCoords();

        float xOffset = rect.width / 4;
        float x = alignment == GameActor.Alignment.Player ? (rect.center.x - xOffset) : (rect.center.x + xOffset);
        float ySpacing = rect.height / slots.Length / 2;
        float yCurrent = rect.center.y + ySpacing * (slots.Length / 2);
        foreach (var slot in slots)
        {
            slot.transform.position = new Vector2(x, yCurrent);
            slot.OnPositionUpdated();  // because Unity doesn't do this for you

            yCurrent -= ySpacing;
        }
    }
    readonly Dictionary<int, GameActorState> actorIdToActor = new Dictionary<int, GameActorState>();
    readonly Dictionary<int, CharacterSlot> actorIdToCharacterSlot = new Dictionary<int, CharacterSlot>();
    class ActorAddedRenderer : IGameEventRenderer
    {
        public readonly GameEventRenderer host;
        public readonly GameActorState actor;
        public ActorAddedRenderer(GameEventRenderer host, GameActor a)
        {
            this.host = host;
            actor = new GameActorState(a);
        }
        public void Render()
        {
            // create a CharacterSlot to host the sprites at the actor location
            var slot = Instantiate(host.assets.CharacterSlotPrefab).GetComponent<CharacterSlot>();

            slot.ShowCharacter(actor.align);
            slot.ShowNameplate(actor.align);
            slot.Nameplate.Name.text = actor.uniqueName;
            slot.Nameplate.HealthBar.Percent = actor.health / actor.baseHealth;

            var id = $"{actor.uniqueName} ({actor.align})";
            slot.name = $"slot {id}";
            slot.Nameplate.name = id;

            var parent = actor.align == GameActor.Alignment.Mob ? host.mobParent : host.playerParent;
            slot.transform.parent = parent.transform;

            host.actorIdToActor[actor.id] = actor;
            host.actorIdToCharacterSlot[actor.id] = slot;

            GameEventRenderer.LayoutActors(parent, actor.align);
        }
    }
    class ActorRemovedRenderer : IGameEventRenderer
    {
        public readonly int id;
        public ActorRemovedRenderer(GameActor a) {  this.id = a.id; }
        public void Render()
        {
            throw new System.NotImplementedException();
        }
    }
    class RoundStartRenderer : IGameEventRenderer
    {
        public readonly int round;
        public RoundStartRenderer(int round) { this.round = round; }
        public void Render()
        {
        }
    }
    class ActorActionsStartRenderer : IGameEventRenderer
    {
        public readonly int actorId;
        public ActorActionsStartRenderer(GameActor a) {  this.actorId = a.id; }
        public void Render()
        {
        }
    }
    class ActorActionsEndRenderer : IGameEventRenderer
    {
        public readonly int actorId;
        public ActorActionsEndRenderer(GameActor a) {  this.actorId = a.id; }
        public void Render()
        {
        }
    }
    class TargetSelectedRenderer : IGameEventRenderer
    {
        public readonly int actorId;
        public readonly int targetId;
        public TargetSelectedRenderer(GameActor a)
        {
            actorId = a.id;
            targetId = a.Target == null ? GameActorState.NULL_ID : a.Target.id;
        }
        public void Render()
        {
        }
    }
    class AttackRenderer : IGameEventRenderer
    {
        public readonly int actorId;
        public readonly int targetId;
        public AttackRenderer(GameActor attacker, GameActor target)
        {
            actorId = attacker.id;
            targetId = target.id;
        }
        public void Render()
        { }
    }
    class HealthChangeRenderer : IGameEventRenderer
    {
        public readonly int actorId;
        public readonly float baseHealth;
        public readonly float before;
        public readonly float after;
        public HealthChangeRenderer(GameActor a, float before, float after)
        {
            actorId = a.id;
            baseHealth = a.baseHealth;
            this.before = before;
            this.after = after;
        }
        public void Render()
        { }
    }
    class DeathRenderer : IGameEventRenderer
    {
        public readonly int actorId;
        public DeathRenderer(GameActor a) { actorId = a.id; }
        public void Render()
        { }
    }
    Queue<IGameEventRenderer> events = new Queue<IGameEventRenderer>();
    void EnqueueEvent(IGameEventRenderer e)
    {
        events.Enqueue(e);
        ui.renderEvent.interactable = events.Count > 0;
        ui.debugText1.text = $"Events: {events.Count}";
    }
    public void RenderEvent()
    {
        var e = events.Dequeue();
        Debug.Log($"rendering {e}");

        ui.renderEvent.interactable = events.Count > 0;
        ui.debugText1.text = $"Events: {events.Count}";

        e.Render();
    }

}