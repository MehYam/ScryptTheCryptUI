using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ScryptTheCrypt;

public class Main : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button proceedButton = null;
    public void RunSmokeTest()
    {
        var game = Util.SampleGame;
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

        var game = Util.SampleGame;
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
    const float animationTime = 2;
    IEnumerator AnimateTargetChoice(string name)
    {
        Debug.LogFormat("AnimateTargetChoice of {0}", name);
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateAttack(string name)
    {
        Debug.LogFormat("AnimateAttack of {0}", name);
        yield return new WaitForSeconds(animationTime);
    }
    IEnumerator AnimateDeath(string name)
    {
        Debug.LogFormat("AnimateDeath of {0}", name);
        yield return new WaitForSeconds(animationTime);
    }
}
