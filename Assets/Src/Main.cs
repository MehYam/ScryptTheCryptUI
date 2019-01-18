using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ScryptTheCrypt;
using ScryptTheCrypt.Actions;

public class Main : MonoBehaviour
{
    void Start()
    {
    }
    public void RunSmokeTest()
    {
        // run some simple tests to create and invoke a Game
        var game = new Game(2112);
        var player = new GameActor("alice");
        var player2 = new GameActor("bob");
        var mob = new GameActor("carly");
        var mob2 = new GameActor("denise");

        // set targeting and affinities
        player.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Mob));
        player2.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Mob));
        mob.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Player));
        mob2.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Player));

        player.AddAction(new ActionAttack());
        player2.AddAction(new ActionAttack());
        mob.AddAction(new ActionAttack());
        mob2.AddAction(new ActionAttack());

        player.Weapon = new GameWeapon("alice's axe", 22);
        player2.Weapon = new GameWeapon("bob's burger", 12);
        mob.Weapon = new GameWeapon("carly's cutlass", 33);
        mob2.Weapon = new GameWeapon("denise's dog", 5);

        game.players.Add(player);
        game.players.Add(player2);
        game.mobs.Add(mob);
        game.mobs.Add(mob2);
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

    }
}
