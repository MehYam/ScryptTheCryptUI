using System;
using System.Collections;
using System.Text;

using UnityEngine;

using kaiGameUtil;
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
    static GameActor[] players = new GameActor[] 
    {
        new GameActor(GameActor.Alignment.Player, "alice"),
        new GameActor(GameActor.Alignment.Player, "bob"),
        new GameActor(GameActor.Alignment.Player, "carly"),
        new GameActor(GameActor.Alignment.Player, "denise"),
        new GameActor(GameActor.Alignment.Player, "edgar"),
        new GameActor(GameActor.Alignment.Player, "faust"),
        new GameActor(GameActor.Alignment.Player, "gabbers"),
        new GameActor(GameActor.Alignment.Player, "heiki"),
        new GameActor(GameActor.Alignment.Player, "ivano"),
        new GameActor(GameActor.Alignment.Player, "jakob"),
        new GameActor(GameActor.Alignment.Player, "kai"),
        new GameActor(GameActor.Alignment.Player, "leo"),
        new GameActor(GameActor.Alignment.Player, "minerva")
    };
    static GameActor[] mobs = new GameActor[] 
    {
        new GameActor(GameActor.Alignment.Mob, "rat", 5),
        new GameActor(GameActor.Alignment.Mob, "goblin", 10),
        new GameActor(GameActor.Alignment.Mob, "spectre", 15),
        new GameActor(GameActor.Alignment.Mob, "skeleton", 20),
        new GameActor(GameActor.Alignment.Mob, "pirate", 50),
        new GameActor(GameActor.Alignment.Mob, "bandit", 55)
    };
    static GameWeapon[] weapons = new GameWeapon[] 
    {
        new GameWeapon("axe", 4),
        new GameWeapon("ballista", 5),
        new GameWeapon("cudgel", 6),
        new GameWeapon("dingbat", 3),
        new GameWeapon("electric sword", 10),
        new GameWeapon("fencing sabre", 7),
        new GameWeapon("gun knife", 4),
        new GameWeapon("helishears", 3),
        new GameWeapon("ignition rod", 7),
        new GameWeapon("jax", 9),
        new GameWeapon("kelvin sapper", 5),
        new GameWeapon("long shiv", 8),
        new GameWeapon("monkey bite", 2)
    };
    static public Game GetSampleGameWithPlayers(RNG rng, int nPlayers)
    {
        var game = new Game(rng);
        for (var i = 0; i < nPlayers; ++i)
        {
            var actorTemplate = Util.players[game.rng.NextIndex(Util.players)];
            var actor = new GameActor(GameActor.Alignment.Player, actorTemplate.name, actorTemplate.baseHealth);

            actor.Weapon = Util.weapons[game.rng.NextIndex(Util.weapons)];
            game.AddActor(actor);
        }
        return game;
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
            var actor = generators[rng.NextIndex(generators)]();
            if (addDefaultAttack)
            {
                actor.SetScrypt(ScryptUtil.defaultAttack);
            }
            return actor;
        }
    }
    static public MobGenerator GetMobGenerator(RNG rng)
    {
        return new MobGenerator(rng, 
            () => new GameActor(GameActor.Alignment.Mob, "rat", 10, new GameWeapon("teeth", 4)),
            () => new GameActor(GameActor.Alignment.Mob, "mole", 8, new GameWeapon("claw", 6)),
            () => new GameActor(GameActor.Alignment.Mob, "lynx", 15, new GameWeapon("pounce", 10))
        );
    }
    static public MobGenerator GetBossGenerator(RNG rng)
    {
        return new MobGenerator(rng,
            () => new GameActor(GameActor.Alignment.Mob, "rat boss", 30, new GameWeapon("gold teeth", 12)),
            () => new GameActor(GameActor.Alignment.Mob, "mole captain", 35, new GameWeapon("poison claw", 14)),
            () => new GameActor(GameActor.Alignment.Mob, "trained lynx", 40, new GameWeapon("swipe", 15))
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
