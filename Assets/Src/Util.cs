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
    static GameActor[] actors = new GameActor[] 
    {
        new GameActor("alice"),
        new GameActor("bob"),
        new GameActor("carly"),
        new GameActor("denise"),
        new GameActor("edgar"),
        new GameActor("faust"),
        new GameActor("gabbers"),
        new GameActor("heiki"),
        new GameActor("ivano"),
        new GameActor("jakob"),
        new GameActor("kai"),
        new GameActor("leo"),
        new GameActor("minerva")
    };
    static GameWeapon[] weapons = new GameWeapon[] 
    {
        new GameWeapon("axe", 22),
        new GameWeapon("ballista", 20),
        new GameWeapon("cudgel", 9),
        new GameWeapon("dingbat", 10),
        new GameWeapon("electric sword", 15),
        new GameWeapon("fencing sabre", 19),
        new GameWeapon("gun knife", 30),
        new GameWeapon("helishears", 31),
        new GameWeapon("ignition rod", 14),
        new GameWeapon("jax", 18),
        new GameWeapon("kelvin sapper", 11),
        new GameWeapon("long shiv", 10),
        new GameWeapon("monkey bite", 18)
    };
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
    static public Game GetSampleGame(int seed, int nActors)
    {
        var game = new Game(seed);
        nActors = Mathf.Min(nActors, weapons.Length);

        for (var i = 0; i < nActors; ++i)
        {
            var actor = Util.actors[i];
            if ((i % 2) == 0) // swap between players and mobs
            {
                actor.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Mob));
                game.players.Add(actor);
            }
            else
            {
                actor.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Player));
                game.mobs.Add(actor);
            }
            actor.AddAction(new ActionAttack());
            actor.Weapon = Util.weapons[i];
        }
        return game;
    }
}
