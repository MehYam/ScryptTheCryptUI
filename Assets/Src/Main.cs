using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using kaiGameUtil;
using ScryptTheCrypt;

public class Main : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button runSmokeTestButton = null;
    [SerializeField] private UnityEngine.UI.Button runAnimatedTestButton = null;
    [SerializeField] private UnityEngine.UI.Button runEnumeratedTestButton = null;
    [SerializeField] private UnityEngine.UI.Button proceedButton = null;
    [SerializeField] private GameObject playerParent = null;
    [SerializeField] private GameObject mobParent = null;
    void Start()
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

        var game = Util.SampleBattle;
        RenderActors(game.Players, Game.ActorAlignment.Player, playerParent);
        RenderActors(game.Mobs, Game.ActorAlignment.Mob, mobParent);

        GameEvents.Instance.AttackEnd += (g, a, b) =>
        {
            Debug.Log($"{a.uniqueName} {a.Health}/{a.baseHealth} attacked {b.uniqueName} {b.Health}/{b.baseHealth}");
        };
        GameEvents.Instance.Death += (g, a) =>
        {
            Debug.Log($"RIP {a.uniqueName}");
        };
        while(game.GameProgress == Game.Progress.InProgress)
        {
            game.PlayRound();
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

        var rng = new RNG(1000);
        var game = Util.GetSampleGameWithPlayers(rng, 3);
        RenderActors(game.Players, Game.ActorAlignment.Player, playerParent);
        RenderActors(game.Mobs, Game.ActorAlignment.Mob, mobParent);

        GameEvents.Instance.ActorTurnStart += (g, a) =>
        {
            animationList.Add(AnimateActorActionsStart(a));
        };
        GameEvents.Instance.ActorTurnEnd += (g, a) =>
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
            animationList.Add(AnimateDeath(a.uniqueName));
        };
        while(game.GameProgress == Game.Progress.InProgress)
        {
            Debug.Log(game.ToString());
            game.PlayRound();

            yield return StartCoroutine(Util.CoroutineSeries(animationList.GetEnumerator()));

            SetWaiting(true);
            yield return new WaitUntil(() => !waitingToProceed);
            SetWaiting(false);
        }
        Debug.Log($"game ended with {game.GameProgress}");

        GameEvents.ReleaseAllListeners();
        TestRunning = false;
    }
    const float animationTime = 1;
    IEnumerator AnimateActorActionsStart(GameActor actor)
    {
        var slot = actorToCharacterSlot[actor];
        slot.ToggleTurnIndicator(true);
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateActorActionsEnd(GameActor actor)
    {
        var slot = actorToCharacterSlot[actor];
        slot.ToggleTurnIndicator(false);

        if (actor.GetAttribute(GameActor.Attribute.Target) is GameActor target)
        {
            var targetSlot = actorToCharacterSlot[target];
            targetSlot.ToggleTargetIndicator(false);
        }

        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateTargetChoice(GameActor actor)
    {
        if (actor.GetAttribute(GameActor.Attribute.Target) is GameActor target) {
            var slot = actorToCharacterSlot[target];
            slot.ToggleTargetIndicator(true);
        }
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateAttack(GameActor attacker, GameActor defender)
    {
        Debug.Log($"{attacker.uniqueName} ATTACKING {defender.uniqueName} for {attacker.Weapon.damage}");
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateHealthChange(GameActor actor, float oldHealth)
    {
        Debug.Log($"{actor.uniqueName} health {oldHealth} => {actor.Health}");
        var slot = actorToCharacterSlot[actor];
        slot.Nameplate.HealthBar.Percent = actor.Health / actor.baseHealth;

        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateDeath(string name)
    {
        Debug.Log($"Death of {name}");
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateEnumeratedGame()
    {
        TestRunning = true;
        var rng = new RNG(1000);
        var game = Util.GetSampleGameWithPlayers(rng, 3);

        GameEvents.Instance.RoundStart += g =>
        {
            Debug.Log("start of turn");
        };
        GameEvents.Instance.ActorTurnStart += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} starts");
            var slot = actorToCharacterSlot[a];
            slot.ToggleTurnIndicator(true);
        };
        GameEvents.Instance.ActorTurnEnd += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} ends");
            var slot = actorToCharacterSlot[a];
            slot.ToggleTurnIndicator(false);

            if (a.GetAttribute(GameActor.Attribute.Target) is GameActor target)
            {
                var targetSlot = actorToCharacterSlot[target];
                targetSlot.ToggleTargetIndicator(false);
            }
        };
        GameEvents.Instance.TargetChosen += (g, a) =>
        {
            Debug.Log($"{a.uniqueName} chooses target {a.GetAttribute(GameActor.Attribute.Target)}");
            if (a.GetAttribute(GameActor.Attribute.Target) is GameActor target) {
                var slot = actorToCharacterSlot[target];
                slot.ToggleTargetIndicator(true);
            }
        };
        GameEvents.Instance.AttackStart += (g, a, b) =>
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
        GameEvents.Instance.Death += (g, a) =>
        {
            Debug.Log($"RIP {a.uniqueName}");
            var slot = actorToCharacterSlot[a];
            slot.ToggleDeathIndicator(true);
        };

        var mobGen = Util.GetMobGenerator(game.rng);
        while (game.GameProgress != Game.Progress.MobsWin)
        {
            // start a new wave
            game.ClearActors(Game.ActorAlignment.Mob);  // have to do this manually for now - maybe move this to Game
            for (int i = 0; i < 4; ++i)
            {
                game.AddActor(mobGen.Gen(true), Game.ActorAlignment.Mob);
            }

            // for simplicity, ditch and re-render everything
            actorToCharacterSlot.Clear();
            RenderActors(game.Players, Game.ActorAlignment.Player, playerParent);
            RenderActors(game.Mobs, Game.ActorAlignment.Mob, mobParent);

            while (game.GameProgress == Game.Progress.InProgress)
            {
                var actions = game.EnumerateRoundActions();
                while (actions.MoveNext())
                {
                    SetWaiting();
                    yield return new WaitUntil(() => !waitingToProceed);
                }
            }
        }
        Debug.Log($"game ended with {game.GameProgress}");
        Debug.Log(game.ToString());

        GameEvents.ReleaseAllListeners();
        TestRunning = false;
    }
    readonly Dictionary<GameActor, CharacterSlot> actorToCharacterSlot = new Dictionary<GameActor, CharacterSlot>();
    void RenderActors(IList<GameActor> actors, Game.ActorAlignment alignment, GameObject parent)
    {
        Util.DestroyAllChildren(parent.transform);

        var rect = Util.GetScreenRectInWorldCoords();
        Debug.Log($"screen rect {rect}");

        float ySpacing = rect.height / actors.Count / 2;
        float x = alignment == Game.ActorAlignment.Player ? -3 : 3;

        var assets = GetComponent<AssetLink>();
        GameObject createSlot(GameActor actor, float y)
        {
            var retval = Instantiate(assets.CharacterSlotPrefab);
            var slot = retval.GetComponent<CharacterSlot>();
            slot.transform.position = new Vector2(x, y);

            Debug.Log($"rendering slot at {slot.transform.position}");

            slot.ShowCharacter(alignment);
            slot.ShowNameplate();
            slot.Nameplate.Name.text = actor.name;
            slot.name = $"slot {actor.uniqueName} ({alignment})";
            retval.transform.parent = parent.transform;

            Debug.Assert(!actorToCharacterSlot.ContainsKey(actor));

            actorToCharacterSlot[actor] = slot;
            return retval;
        }

        float yCurrent = rect.center.y + ySpacing * (actors.Count / 2) - 1;
        foreach(var actor in actors)
        {
            var slot = createSlot(actor, yCurrent);
            yCurrent -= ySpacing;
        }
    }
}
