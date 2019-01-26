using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ScryptTheCrypt;

public class Main : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button runSmokeTestButton = null;
    [SerializeField] private UnityEngine.UI.Button runAnimatedTestButton = null;
    [SerializeField] private UnityEngine.UI.Button runEnumeratedTestButton = null;
    [SerializeField] private UnityEngine.UI.Button proceedButton = null;
    [SerializeField] private GameObject spriteParent = null;
    private void Start()
    {
        TestRunning = false;
    }
    private bool TestRunning
    {
        get
        {
            return !runSmokeTestButton.interactable;
        }
        set
        {
            runSmokeTestButton.interactable = !value;
            runAnimatedTestButton.interactable = !value;
            runEnumeratedTestButton.interactable = !value;
        }
    }
    public void RunSmokeTest()
    {
        TestRunning = true;

        var game = Util.SampleGame;
        RenderGame(game);

        GameEvents.Instance.AttackEnd += (g, a, b) =>
        {
            Debug.Log($"{a.name} {a.Health}/{a.baseHealth} attacked {b.name} {b.Health}/{b.baseHealth}");
        };
        GameEvents.Instance.Death += (g, a) =>
        {
            Debug.Log($"RIP {a.name}");
        };
        while(game.GameProgress == Game.Progress.InProgress)
        {
            game.DoTurn();
        }
        Debug.Log($"game ended with {game.GameProgress}");

        TestRunning = false;
    }
    public void RunAnimatedTest()
    {
        StartCoroutine(AnimateGame());
    }
    public void RunEnumeratedTest()
    {
        StartCoroutine(AnimateEnumeratedGame());
    }
    bool waitingToProceed = false;
    public void SetWaiting(bool waiting = true)
    {
        waitingToProceed = waiting;
        proceedButton.interactable = waiting;
    }
    IEnumerator AnimateGame()
    {
        TestRunning = true;
        var animationList = new List<IEnumerator>();

        var game = Util.GetSampleGame(1000, 6);
        RenderGame(game);

        GameEvents.Instance.ActorActionsStart += (g, a) =>
        {
            animationList.Add(AnimateActorActionsStart(a));
        };
        GameEvents.Instance.ActorActionsEnd += (g, a) =>
        {
            animationList.Add(AnimateActorActionsEnd(a));
        };
        GameEvents.Instance.TargetChosen += (g, a) =>
        {
            animationList.Add(AnimateTargetChoice(a));
        };
        GameEvents.Instance.AttackStart += (g, a, b) =>
        {
            animationList.Add(AnimateAttack(a, b));
        };
        GameEvents.Instance.ActorHealthChange += (a, oldHealth, newHealth) =>
        {
            animationList.Add(AnimateHealthChange(a, oldHealth));
        };
        GameEvents.Instance.Death += (g, a) =>
        {
            animationList.Add(AnimateDeath(a.name));
        };
        while(game.GameProgress == Game.Progress.InProgress)
        {
            Debug.Log(game.ToString());
            game.DoTurn();

            yield return StartCoroutine(Util.CoroutineSeries(animationList.GetEnumerator()));

            SetWaiting(true);
            yield return new WaitUntil(() => !waitingToProceed);
            SetWaiting(false);
        }
        Debug.Log($"game ended with {game.GameProgress}");

        GameEvents.ReleaseAllListeners();
        TestRunning = false;
    }
    IEnumerator AnimateEnumeratedGame()
    {
        TestRunning = true;
        var game = Util.GetSampleGame(1000, 6);
        RenderGame(game);

        GameEvents.Instance.TurnStart += g =>
        {
            Debug.Log("start of turn");
        };
        GameEvents.Instance.ActorActionsStart += (g, a) =>
        {
            var slot = actorToCharacterSlot[a];
            slot.ShowTurnIndicator(true);
        };
        GameEvents.Instance.ActorActionsEnd += (g, a) =>
        {
            var slot = actorToCharacterSlot[a];
            slot.ShowTurnIndicator(false);
        };
        GameEvents.Instance.TargetChosen += (g, a) =>
        {
            AnimateTargetChoice(a);
        };
        GameEvents.Instance.AttackStart += (g, a, b) =>
        {
            AnimateAttack(a, b);
        };
        GameEvents.Instance.ActorHealthChange += (a, oldHealth, newHealth) =>
        {
            var slot = actorToCharacterSlot[a];
            slot.Nameplate.HealthBar.Percent = a.Health / a.baseHealth;
        };
        GameEvents.Instance.Death += (g, a) =>
        {
            AnimateDeath(a.name);
        };

        while(game.GameProgress == Game.Progress.InProgress)
        {
            var turns = game.EnumerateTurns();
            while (turns.MoveNext())
            {
                yield return new WaitForEndOfFrame();
            }
        }
        Debug.Log($"game ended with {game.GameProgress}");
        Debug.Log(game.ToString());

        GameEvents.ReleaseAllListeners();
        TestRunning = false;
    }
    const float animationTime = 1;
    IEnumerator AnimateActorActionsStart(GameActor actor)
    {
        var slot = actorToCharacterSlot[actor];
        slot.ShowTurnIndicator(true);
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateActorActionsEnd(GameActor actor)
    {
        var slot = actorToCharacterSlot[actor];
        slot.ShowTurnIndicator(false);

        if (actor.GetAttribute(GameActor.Attribute.Target) is GameActor target)
        {
            var targetSlot = actorToCharacterSlot[target];
            targetSlot.ShowTargetIndicator(false);
        }

        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateTargetChoice(GameActor actor)
    {
        if (actor.GetAttribute(GameActor.Attribute.Target) is GameActor target) {
            var slot = actorToCharacterSlot[target];
            slot.ShowTargetIndicator(true);
        }
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateAttack(GameActor attacker, GameActor defender)
    {
        Debug.Log($"{attacker.name} ATTACKING {defender.name} for {attacker.Weapon.damage}");
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateHealthChange(GameActor actor, float oldHealth)
    {
        Debug.Log($"{actor.name} health {oldHealth} => {actor.Health}");
        var slot = actorToCharacterSlot[actor];
        slot.Nameplate.HealthBar.Percent = actor.Health / actor.baseHealth;

        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateDeath(string name)
    {
        Debug.Log($"Death of {name}");
        yield return new WaitForSeconds(animationTime);
    }
    readonly Dictionary<GameActor, CharacterSlot> actorToCharacterSlot = new Dictionary<GameActor, CharacterSlot>();
    void RenderGame(Game game)
    {
        var assets = GetComponent<AssetLink>();

        actorToCharacterSlot.Clear();

        const float xPlayers = -3;
        const float xMobs = 3;
        const float ySpacing = 3;

        float yStart = 1.5f-(game.players.Count * ySpacing) / 2;
        GameObject createSlot(GameActor actor, Game.ActorAlignment mobType, float x, float y)
        {
            var retval = Instantiate(assets.CharacterSlotPrefab);
            var slot = retval.GetComponent<CharacterSlot>();
            slot.transform.position = new Vector2(x, y);

            slot.ShowCharacter(mobType);
            slot.ShowNameplate();
            slot.Nameplate.Name.text = actor.name;
            retval.transform.parent = spriteParent.transform;

            actorToCharacterSlot[actor] = slot;
            return retval;
        }
        foreach(var player in game.players)
        {
            var slot = createSlot(player, Game.ActorAlignment.Player, xPlayers, yStart);
            yStart += ySpacing;
        }
        yStart = 1.5f-(game.mobs.Count * ySpacing) / 2;
        foreach(var mob in game.mobs)
        {
            var slot = createSlot(mob, Game.ActorAlignment.Mob, xMobs, yStart);
            yStart += ySpacing;
        }
    }
}
