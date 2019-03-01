using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using ScryptTheCrypt;

public class Main : MonoBehaviour
{
    [SerializeField] private GameObject playerParent = null;
    [SerializeField] private GameObject mobParent = null;
    [SerializeField] private uint heroes = 4;
    [SerializeField] private uint mobsPerWave = 4;
    [SerializeField] private uint bossWave = 5;
    [SerializeField] private int seed = 2112;
    [SerializeField] private float delayBetweenActions = 0.5f;

    private MainAssetLink assets;
    private MainUILink ui;
    void Start()
    {
        assets = GetComponent<MainAssetLink>();
        ui = GetComponent<MainUILink>();

        SetRunState(RunState.Idle);
    }
    void OnDestroy()
    {
        GameEvents.ReleaseAllListeners();
    }
    public void RunAction()
    {
        SetRunState(RunState.RunningAction);
    }
    public void RunRound()
    {
        SetRunState(RunState.RunningRound);
    }
    Coroutine _coroutine;
    enum RunState { Idle, RunningAction, RunningRound };
    private RunState _runState;
    private void SetRunState(RunState state)
    {
        Debug.Log($"SetRunState {state}");
        bool idle = state == RunState.Idle;
        ui.runActionButton.interactable = idle;
        ui.runRound.interactable = idle;

        _runState = state;
        if (_coroutine == null && state != RunState.Idle)
        {
            _coroutine = StartCoroutine(RunGame_v2_WithRenderers());
        }
    }
    int nCurrentWave = 0;
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Event queue-based implementation
    /// </summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////
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
    }
    IEnumerator RunGame_v2_WithRenderers()
    {
        GameEvents.Instance.ActorAdded += (g, a) =>
        {
            EnqueueEvent(new ActorAddedRenderer(a));
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

        var rng = new RNG(seed);
        var game = Util.GetSampleGameWithPlayers(rng, 3);
        var mobGen = Util.GetMobGenerator(game.rng);
        var bossGen = Util.GetBossGenerator(game.rng);

        // the mobs always win, play until they do
        nCurrentWave = 0;
        while (game.GameProgress != Game.Progress.MobsWin) // loop over waves
        {
            // new wave of mobs
            ++nCurrentWave;
            Debug.Log($"=-=-=-=-=-=-= starting wave {nCurrentWave} =-=-=-=-=-=");

            game.ClearActors(GameActor.Alignment.Mob);

            if ((++nCurrentWave % bossWave) == 0)
            {
                var boss = bossGen.Gen(true);
                Debug.Log($"ADDING BOSS {boss}");

                game.AddActor(boss);
            }
            else
            {
                for (int i = 0; i < mobsPerWave; ++i)
                {
                    game.AddActor(mobGen.Gen(true));
                }
            }

            while (game.GameProgress == Game.Progress.InProgress) // loop over rounds until wave is clear or players are dead
            {
                Debug.Log($"=-=-=-=-=-==--=- invoking round {game.NumRounds} =-=--=-=-=-=-");
                var actions = game.EnumerateRound_Scrypt();
                while (actions.MoveNext())
                {
                }
                Debug.Log($"=-=-=-=-=-=-=-=- round {game.NumRounds} complete ==-=-=-=-=-=-");

                SetRunState(RunState.Idle);
                yield return new WaitUntil(() => _runState != RunState.Idle);
            }
            Debug.Log($"=-=-==-=-=- round ended with {game.GameProgress} =-=-=-=-=-=-=-=-=");
        }
        Debug.Log($"=-=-=-=-==-=- game ended with {game.GameProgress} =-=-=-=-=-=-");
        yield return null;
    }
    IEnumerator RunGame()
    {
        var rng = new RNG(seed);
        var game = Util.GetSampleGameWithPlayers(rng, 3);

        GameEvents.Instance.RoundStart += g =>
        {
            Debug.Log("start of turn");
            ui.debugText1.text = $"Wave {nCurrentWave}, Round {game.NumRounds}";
        };
        GameEvents.Instance.ActorActionsStart += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} starts");

            ui.debugText2.text = $"{a.uniqueName}'s turn";

            var slot = actorToCharacterSlot[a];
            slot.ToggleTurnIndicator(true);
        };
        GameEvents.Instance.ActorActionsEnd += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} ends");
            var slot = actorToCharacterSlot[a];
            slot.ToggleTurnIndicator(false);

            if (a.Target != null)
            {
                var targetSlot = actorToCharacterSlot[a.Target];
                targetSlot.ToggleTargetIndicator(false);
            }
        };
        GameEvents.Instance.TargetSelected += a =>
        {
            Debug.Log($"{a.uniqueName} chooses target {a.Target}");
            if (a.Target != null) {
                var slot = actorToCharacterSlot[a.Target];
                slot.ToggleTargetIndicator(true);
            }
        };
        GameEvents.Instance.AttackStart += (a, b) =>
        {
            Debug.Log($"{a.uniqueName} attacks {b.uniqueName}");
            var slot = actorToCharacterSlot[b];
            slot.ShowDamageText(a.Weapon.damage.ToString());
        };
        GameEvents.Instance.ActorHealthChange += (a, oldHealth, newHealth) =>
        {
            Debug.Log($"{a.uniqueName} health {oldHealth} => {newHealth}");
            var slot = actorToCharacterSlot[a];
            slot.Nameplate.HealthBar.Percent = a.Health / a.baseHealth;
        };
        GameEvents.Instance.Death += a =>
        {
            Debug.Log($"RIP {a.uniqueName}");
            var slot = actorToCharacterSlot[a];
            slot.ToggleDeathIndicator(true);
        };

        var mobGen = Util.GetMobGenerator(game.rng);
        var bossGen = Util.GetBossGenerator(game.rng);

        nCurrentWave = 0;
        while (game.GameProgress != Game.Progress.MobsWin)
        {
            // start a new mob wave
            game.ClearActors(GameActor.Alignment.Mob);  // have to do this manually for now - maybe move this to Game

            if ((++nCurrentWave % bossWave) == 0)
            {
                var boss = bossGen.Gen(true);
                Debug.Log($"ADDING BOSS {boss}");

                game.AddActor(boss);
            }
            else
            {
                for (int i = 0; i < mobsPerWave; ++i)
                {
                    game.AddActor(mobGen.Gen(true));
                }
            }
            // for simplicity, ditch and re-render everything
            foreach (var slot in actorToCharacterSlot.Values)
            {
                GameObject.Destroy(slot.gameObject);
            }
            actorToCharacterSlot.Clear();
            RenderActors(game.Players, GameActor.Alignment.Player, playerParent);
            RenderActors(game.Mobs, GameActor.Alignment.Mob, mobParent);

            Debug.Log($"=-=-=-=-spawned new wave size {actorToCharacterSlot.Values.Count}=-=-=-=-=-=-=-=");

            while (game.GameProgress == Game.Progress.InProgress)
            {
                Debug.Log("=-=-=-=-=-=-= starting round =-=-=-=-=-=-=-=");
                var actions = game.EnumerateRound_Scrypt();
                while (actions.MoveNext())
                {
                    Debug.Log($"--- next action, runstate {_runState}");
                    if (_runState == RunState.RunningAction)
                    {
                        SetRunState(RunState.Idle);
                    }
                    else if (_runState == RunState.RunningRound)
                    {
                        yield return new WaitForSeconds(delayBetweenActions);
                    }
                    yield return new WaitUntil(() => _runState != RunState.Idle);
                }
                Debug.Log("=-=-=-=-=-=-=-= done round -=-=-=-=-=-=-==-=-");

                SetRunState(RunState.Idle);
                yield return new WaitUntil(() => _runState != RunState.Idle);
            }
            Debug.Log($"=-=-=-=-=-=wave ended with {game.GameProgress}=-=-=-=-=-=-=-=");
        }
        var result = $"game ended with {game.GameProgress}";
        Debug.Log(result);
        Debug.Log(game.ToString());

        ui.debugText2.text = result;
        GameEvents.ReleaseAllListeners();
    }
    /// <summary>
    /// //////////////////// OLD IMPLEMENTATION ////////////////////////////////////////////
    /// </summary>
    readonly Dictionary<GameActor, CharacterSlot> actorToCharacterSlot = new Dictionary<GameActor, CharacterSlot>();
    void RenderActors(IList<GameActor> actors, GameActor.Alignment alignment, GameObject parent)
    {
        var rect = Util.GetScreenRectInWorldCoords();
        Debug.Log($"screen rect {rect}");

        float ySpacing = rect.height / actors.Count / 2;
        float x = alignment == GameActor.Alignment.Player ? -3 : 3;

        var assets = GetComponent<MainAssetLink>();
        GameObject createSlot(GameActor actor, float y)
        {
            var retval = Instantiate(assets.CharacterSlotPrefab);
            var slot = retval.GetComponent<CharacterSlot>();
            slot.transform.position = new Vector2(x, y);

            slot.ShowCharacter(alignment);
            slot.ShowNameplate(alignment);
            slot.Nameplate.Name.text = actor.name;
            slot.Nameplate.HealthBar.Percent = actor.Health / actor.baseHealth;

            var id = $"{actor.uniqueName} ({alignment})";
            slot.name = $"slot {id}";
            slot.Nameplate.name = id;
            retval.transform.parent = parent.transform;

            if (actorToCharacterSlot.ContainsKey(actor))
            {
                Debug.LogError($"{actor.uniqueName} already in actorToCharacterSlot");
                Debug.Assert(!actorToCharacterSlot.ContainsKey(actor));
            }

            actorToCharacterSlot[actor] = slot;
            return retval;
        }

        float yCurrent = rect.center.y + ySpacing * (actors.Count / 2);
        foreach(var actor in actors)
        {
            var slot = createSlot(actor, yCurrent);
            yCurrent -= ySpacing;
        }
    }
}
