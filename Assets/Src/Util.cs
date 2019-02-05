using System;
using System.Collections;
using UnityEngine;

using ScryptTheCrypt;
using ScryptTheCrypt.Actions;

using kaiGameUtil;

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
        new GameWeapon("axe", 4),
        new GameWeapon("ballista", 5),
        new GameWeapon("cudgel", 6),
        new GameWeapon("dingbat", 3),
        new GameWeapon("electric sword", 10),
        new GameWeapon("fencing sabre", 9),
        new GameWeapon("gun knife", 4),
        new GameWeapon("helishears", 8),
        new GameWeapon("ignition rod", 7),
        new GameWeapon("jax", 9),
        new GameWeapon("kelvin sapper", 11),
        new GameWeapon("long shiv", 10),
        new GameWeapon("monkey bite", 2)
    };
    static public Game SampleBattle
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

            game.Players.Add(player);
            game.Players.Add(player2);
            game.Mobs.Add(mob);
            game.Mobs.Add(mob2);

            return game;
        }
    }
    public class MobGenerator
    {
        private readonly Func<GameActor>[] generators;
        private readonly RNG rng;
        public MobGenerator(RNG rng, params Func<GameActor>[] generators)
        {
            this.rng = rng;
            this.generators = generators;
        }
        public GameActor Gen(bool addDefaultAttack)
        {
            var retval = generators[rng.NextIndex(generators)]();
            if (addDefaultAttack)
            {
                retval.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Player));
                retval.AddAction(new ActionAttack());
            }
            return retval;
        }
    }
    static public Game GetSampleGameWithPlayers(RNG rng, int nPlayers)
    {
        var game = new Game(rng);
        for (var i = 0; i < nPlayers; ++i)
        {
            var actorTemplate = Util.actors[game.rng.NextIndex(Util.actors)];
            var actor = new GameActor(actorTemplate.name, actorTemplate.baseHealth);

            actor.AddAction(new ActionChooseRandomTarget(Game.ActorAlignment.Mob));
            actor.AddAction(new ActionAttack());
            actor.Weapon = Util.weapons[game.rng.NextIndex(Util.weapons)];
            game.AddActor(actor, Game.ActorAlignment.Player);
        }
        return game;
    }
    static public MobGenerator GetMobGenerator(RNG rng)
    {
        return new MobGenerator(rng, 
            () => new GameActor("rat", 10, new GameWeapon("teeth", 4)),
            () => new GameActor("mole", 8, new GameWeapon("claw", 6)),
            () => new GameActor("lynx", 15, new GameWeapon("pounce", 10))
        );
    }
    static public Rect GetScreenRectInWorldCoords()
    {
        float height = Camera.main.orthographicSize * 2.0f;
        float width = height * Screen.width / Screen.height;

        var pos = Camera.main.transform.position;
        pos.x = -width / 2;
        pos.y = -height / 2;
        return new Rect(pos, new Vector2(width, height));
    }
    static public void DestroyAllChildren(Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
