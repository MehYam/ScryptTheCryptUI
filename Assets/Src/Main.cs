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
            animationList.Add(AnimateActorActionsStart(a.name));
        };
        GameEvents.Instance.TargetChosen += (g, a) =>
        {
            animationList.Add(AnimateTargetChoice(a.name));
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
    IEnumerator AnimateActorActionsStart(string name)
    {
        Debug.LogFormat("ActionsStart of {0}", name);
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateTargetChoice(string name)
    {
        Debug.LogFormat("TargetChoice of {0}", name);
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
    void RenderGame(Game game)
    {
        var assets = GetComponent<AssetLink>();

        const float xPlayers = -3;
        const float xMobs = 3;
        const float ySpacing = 2;

        float yStart = -(game.players.Count * ySpacing) / 2;
        GameObject createSprite(GameObject prefab)
        {
            var retval = Instantiate(assets.PlayerSprite);
            retval.layer = LayerMask.NameToLayer("CharacterLayer");
            retval.GetComponent<SpriteRenderer>().sortingLayerName = "Character";

            retval.transform.parent = spriteParent.transform;
            return retval;
        }
        foreach(var player in game.players)
        {
            var playerObj = createSprite(assets.PlayerSprite);
            playerObj.transform.position = new Vector2(xPlayers, yStart);
            yStart += ySpacing;
        }
        yStart = -(game.mobs.Count * ySpacing) / 2;
        foreach(var player in game.players)
        {
            var playerObj = createSprite(assets.MobSprite);
            playerObj.transform.position = new Vector2(xMobs, yStart);
            yStart += ySpacing;
        }
    }
}
