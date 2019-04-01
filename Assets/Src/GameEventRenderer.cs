using System.Collections.Generic;
using UnityEngine;

using KaiGameUtil;
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
            EnqueueEvent(new ActorPositionRenderer(this, a));
        };
        GameEvents.Instance.ActorDirectionChange += (a, old) =>
        {
            if (actorIdToCharacterSlot.ContainsKey(a.id))
            {
                EnqueueEvent(new ActorPositionRenderer(this, a));
            }
        };
        GameEvents.Instance.ActorPositionChange += (a, old) =>
        {
            if (actorIdToCharacterSlot.ContainsKey(a.id))
            {
                EnqueueEvent(new ActorPositionRenderer(this, a));
            }
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
            EnqueueEvent(new ActorActionsStartRenderer(this, a));
        };
        GameEvents.Instance.ActorActionsEnd += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} ends");
            EnqueueEvent(new ActorActionsEndRenderer(this, a));
        };
        GameEvents.Instance.ActorTargetedChange += a =>
        {
            Debug.Log($"{a.uniqueName} targeted {a.Targeted}");
            EnqueueEvent(new TargetedChangeRenderer(this, a));
        };
        GameEvents.Instance.AttackStart += (a, b) =>
        {
            Debug.Log($"{a.uniqueName} attacks {b.uniqueName}");
            EnqueueEvent(new AttackRenderer(this, a, b));
        };
        GameEvents.Instance.ActorHealthChange += (a, oldHealth, newHealth) =>
        {
            Debug.Log($"{a.uniqueName} health {oldHealth} => {newHealth}");
            EnqueueEvent(new HealthChangeRenderer(this, a, oldHealth, newHealth));
        };
        GameEvents.Instance.Death += a =>
        {
            Debug.Log($"RIP {a.uniqueName}");
            EnqueueEvent(new DeathRenderer(this, a));
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
        public readonly Point<int> pos;
        public readonly Point<int> dir;
        public GameActorState(GameActor actor)
        {
            id = actor.id;
            name = actor.name;
            uniqueName = actor.uniqueName;
            align = actor.align;
            baseHealth = actor.baseHealth;
            health = actor.Health;
            pos = actor.pos;
            dir = actor.dir;
        }
    }
    static private void LayoutActors(GameObject actorParent, GameActor.Alignment alignment)
    {
        var slots = actorParent.GetComponentsInChildren<CharacterSlot>();

        var rect = Util.GetScreenRectInWorldCoords();

        float xOffset = rect.width / 4;
        float x = alignment == GameActor.Alignment.Player ? (rect.center.x - xOffset) : (rect.center.x + xOffset);
        float nSlots = (float)slots.Length;
        float screenHeightMinusMargin = rect.height - 1;
        float slotHeight = screenHeightMinusMargin / nSlots;
        float yCurrent = rect.center.y + ((nSlots - 1)/2) * slotHeight;
        foreach (var slot in slots)
        {
            slot.transform.position = new Vector2(x, yCurrent);
            slot.OnPositionUpdated();  // because Unity doesn't do this for you

            yCurrent -= slotHeight;
        }
    }
    readonly Dictionary<int, GameActorState> actorIdToActor = new Dictionary<int, GameActorState>();
    readonly Dictionary<int, CharacterSlot> actorIdToCharacterSlot = new Dictionary<int, CharacterSlot>();
    class ActorAddedRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        readonly GameActorState actor;
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
        }
    }
    class ActorPositionRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        readonly GameActorState actor;
        public ActorPositionRenderer(GameEventRenderer host, GameActor a)
        {
            this.host = host;
            actor = new GameActorState(a);
        }
        public void Render()
        {
            var slot = host.actorIdToCharacterSlot[actor.id];

            slot.transform.position = new Vector2(actor.pos.x, actor.pos.y);
            if (actor.dir == PointUtil.left)
            {
                var scale = slot.transform.localScale;
                scale.x *= -1;
                slot.transform.localScale = scale;
            }
            slot.OnPositionUpdated();
        }
    }
    class ActorRemovedRenderer : IGameEventRenderer
    {
        readonly int id;
        public ActorRemovedRenderer(GameActor a) {  this.id = a.id; }
        public void Render()
        {
            Debug.LogWarning($"not implemented: {this.GetType().Name}");
        }
    }
    class RoundStartRenderer : IGameEventRenderer
    {
        readonly int round;
        public RoundStartRenderer(int round) { this.round = round; }
        public void Render()
        {
            Debug.LogWarning($"not implemented: {this.GetType().Name}");
        }
    }
    class ActorActionsStartRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        readonly int actorId;
        public ActorActionsStartRenderer(GameEventRenderer host, GameActor a) {  this.host = host; this.actorId = a.id; }
        public void Render()
        {
            var slot = host.actorIdToCharacterSlot[actorId];
            slot.ToggleTurnIndicator(true);
        }
    }
    class ActorActionsEndRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        readonly int actorId;
        public ActorActionsEndRenderer(GameEventRenderer host, GameActor a) {  this.host = host; this.actorId = a.id; }
        public void Render()
        {
            var slot = host.actorIdToCharacterSlot[actorId];
            slot.ToggleTurnIndicator(false);
        }
    }
    class TargetedChangeRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        public readonly int actorId;
        public readonly bool targeted;
        public TargetedChangeRenderer(GameEventRenderer host, GameActor a)
        {
            this.host = host;
            actorId = a.id;
            targeted = a.Targeted;
        }
        public void Render()
        {
            var slot = host.actorIdToCharacterSlot[actorId];
            slot.ToggleTargetIndicator(targeted);
        }
    }
    class AttackRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        readonly int actorId;
        readonly int targetId;
        public AttackRenderer(GameEventRenderer host, GameActor attacker, GameActor target)
        {
            this.host = host;
            actorId = attacker.id;
            targetId = target.id;
        }
        public void Render()
        {
            Debug.LogWarning($"not implemented: {this.GetType().Name}");
        }
    }
    class HealthChangeRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        readonly int actorId;
        readonly float baseHealth;
        readonly float before;
        readonly float after;
        public HealthChangeRenderer(GameEventRenderer host, GameActor a, float before, float after)
        {
            this.host = host;
            actorId = a.id;
            baseHealth = a.baseHealth;
            this.before = before;
            this.after = after;
        }
        public void Render()
        {
            var slot = host.actorIdToCharacterSlot[actorId];
            slot.ShowDamageText((before - after).ToString());
            slot.Nameplate.HealthBar.Percent = after / baseHealth;
        }
    }
    class DeathRenderer : IGameEventRenderer
    {
        readonly GameEventRenderer host;
        readonly int actorId;
        public DeathRenderer(GameEventRenderer host, GameActor a) { this.host = host; actorId = a.id; }
        public void Render()
        {
            var slot = host.actorIdToCharacterSlot[actorId];
            slot.ToggleDeathIndicator(true);
        }
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