using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ScryptTheCrypt;

public class Main : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button proceedButton;
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
        proceedButton.interactable = true;

        StartCoroutine(AnimatedTest());
    }
    bool animatedTestProceed = false;
    IEnumerator AnimatedTest()
    {
        var game = Util.SampleGame;
        GameEvents.Instance.TargetChosen += (g, a) =>
        {
            Debug.LogFormat("{0} chooses target {1}", a.name, ((GameActor)a.GetAttribute(GameActor.Attribute.Target)).name);
        };
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
            Debug.Log(game.ToString());
            game.DoTurn();

            yield return new WaitUntil(() => animatedTestProceed);
            animatedTestProceed = false;
        }
        Debug.LogFormat("game ended with {0}", game.GameProgress);

        GameEvents.ReleaseAllListeners();
        yield return null;
    }
    public void AnimatedTestProceed()
    {
        animatedTestProceed = true;
    }
}
