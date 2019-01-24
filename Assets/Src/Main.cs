using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ScryptTheCrypt;

public class Main : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button proceedButton = null;
    [SerializeField] private GameObject spriteParent = null;
    public void RunSmokeTest()
    {
        var game = Util.SampleGame;
        RenderGame(game);

        GameEvents.Instance.AttackEnd += (g, a, b) =>
        {
            Debug.LogFormat("{0} {1}/{2} attacked {3} {4}/{5}", a.name, a.Health, a.baseHealth, b.name, b.Health, b.baseHealth);
        };
        GameEvents.Instance.Death += (g, a) =>
        {
            Debug.LogFormat("RIP {0}", a.name);
        };
        while(game.GameProgress == Game.Progress.InProgress)
        {
            game.DoTurn();
        }
        Debug.LogFormat("game ended with {0}", game.GameProgress);
    }
    public void RunAnimatedTest()
    {
        StartCoroutine(AnimateTest());
    }
    bool waitingToProceed = false;
    public void SetWaiting(bool waiting = true)
    {
        waitingToProceed = waiting;
        proceedButton.interactable = waiting;
    }
    IEnumerator AnimateTest()
    {
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
        GameEvents.Instance.AttackEnd += (g, a, b) =>
        {
            animationList.Add(AnimateAttack(a.name));
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
        Debug.LogFormat("game ended with {0}", game.GameProgress);

        GameEvents.ReleaseAllListeners();
        yield return null;
    }
    const float animationTime = 1;
    IEnumerator AnimateActorActionsStart(GameActor actor)
    {
        Debug.LogFormat("ActionsStart of {0}", actor.name);

        var slot = actorToCharacterSlot[actor];
        slot.ShowTurnIndicator(true);

        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateActorActionsEnd(GameActor actor)
    {
        Debug.LogFormat("ActionsEnd of {0}", actor.name);

        var slot = actorToCharacterSlot[actor];
        slot.ShowTurnIndicator(false);

        GameActor target = actor.GetAttribute(GameActor.Attribute.Target) as GameActor;
        if (target != null)
        {
            var targetSlot = actorToCharacterSlot[target];
            targetSlot.ShowTargetIndicator(false);
        }

        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateTargetChoice(GameActor actor)
    {
        Debug.LogFormat("TargetChoice of {0}", actor.name);

        GameActor target = actor.GetAttribute(GameActor.Attribute.Target) as GameActor;
        if (target != null) {
            var slot = actorToCharacterSlot[target];
            slot.ShowTargetIndicator(true);
        }
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateAttack(string name)
    {
        Debug.LogFormat("Attack of {0}", name);
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateDeath(string name)
    {
        Debug.LogFormat("Death of {0}", name);
        yield return new WaitForSeconds(animationTime);
    }
    readonly Dictionary<GameActor, CharacterSlot> actorToCharacterSlot = new Dictionary<GameActor, CharacterSlot>();
    void RenderGame(Game game)
    {
        var assets = GetComponent<AssetLink>();

        actorToCharacterSlot.Clear();

        const float xPlayers = -3;
        const float xMobs = 3;
        const float ySpacing = 2;

        float yStart = -(game.players.Count * ySpacing) / 2;
        GameObject createSlot(GameActor actor, Game.ActorAlignment mobType)
        {
            var retval = Instantiate(assets.CharacterSlotPrefab);
            var slot = retval.GetComponent<CharacterSlot>();
            slot.ShowCharacter(mobType);
            retval.transform.parent = spriteParent.transform;

            actorToCharacterSlot[actor] = slot;
            return retval;
        }
        foreach(var player in game.players)
        {
            var playerObj = createSlot(player, Game.ActorAlignment.Player);
            playerObj.transform.position = new Vector2(xPlayers, yStart);
            yStart += ySpacing;
        }
        yStart = -(game.mobs.Count * ySpacing) / 2;
        foreach(var mob in game.mobs)
        {
            var playerObj = createSlot(mob, Game.ActorAlignment.Mob);
            playerObj.transform.position = new Vector2(xMobs, yStart);
            yStart += ySpacing;
        }
    }
}
