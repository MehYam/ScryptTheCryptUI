using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using ScryptTheCrypt;

public class Main : MonoBehaviour
{
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
    IEnumerator RunGame_v2_WithRenderers()
    {
        var rendererHost = GetComponent<GameEventRenderer>();

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
    readonly Dictionary<GameActor, CharacterSlot> actorToCharacterSlot = new Dictionary<GameActor, CharacterSlot>();
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
            //RenderActors(game.Players, GameActor.Alignment.Player, playerParent);
            //RenderActors(game.Mobs, GameActor.Alignment.Mob, mobParent);

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
}
