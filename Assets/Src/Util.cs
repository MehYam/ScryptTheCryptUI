using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ScryptTheCrypt;
using ScryptTheCrypt.Actions;

public static class Util
{
    // brilliant idea from https://gamedev.stackexchange.com/questions/63721/coroutines-in-series
    static public IEnumerator CoroutineSeries(params IEnumerator[] series)
    {
        // in a nutshell:  this works with StartCoroutine by returning an enumerator of enumerators.
        //
        // IEnumerator +yield cause a function scope to be compiled as an iterator instance.  Any
        // arguments passed into that function are stored in the instance, so it functions a little
        // like a closure or C++ lambda.  MoveNext(), called internally by Unity after StartCoroutine,
        // or in a foreach loop, execute the actual code of the function.
        foreach(var routine in series)
        {
            while (routine.MoveNext())
            {
                yield return routine.Current;
            }
        }
    }
    static public Game SampleGame
    {
        get
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

            return game;
        }
    }
}
