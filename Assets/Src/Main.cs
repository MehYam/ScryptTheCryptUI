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

    private MainUILink ui;
    void Start()
    {
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
            _coroutine = StartCoroutine(RunGameModel());
        }
    }
    int nCurrentWave = 0;
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Event queue-based implementation
    /// </summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////
    IEnumerator RunGameModel()
    {
        var rendererHost = GetComponent<GameEventRenderer>();

        var rng = new RNG(seed);
        var game = Util.GetSampleGameWithPlayers(rng, heroes);
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
                    if (_runState == RunState.RunningAction)
                    {
                        Debug.Log("action run, waiting");

                        SetRunState(RunState.Idle);
                        yield return new WaitUntil(() => _runState != RunState.Idle);
                    }
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
}
