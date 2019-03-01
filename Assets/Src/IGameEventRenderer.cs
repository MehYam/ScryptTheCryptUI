
using ScryptTheCrypt;

public interface IGameEventRenderer 
{
    void Render();
}

public struct GameActorState
{
    public const int NULL_ID = -1;

    public readonly int id;
    public readonly string name;
    public readonly string uniqueName;
    public readonly float baseHealth;
    public readonly float health;

    public GameActorState(GameActor actor)
    {
        id = actor.id;
        name = actor.name;
        uniqueName = actor.uniqueName;
        baseHealth = actor.baseHealth;
        health = actor.Health;
    }
}
public class ActorAddedRenderer : IGameEventRenderer
{
    public readonly GameActorState state;

    public ActorAddedRenderer(GameActor a)
    {
        state = new GameActorState(a);
    }
    public void Render()
    {
    }
}
public class ActorRemovedRenderer : IGameEventRenderer
{
    public readonly int id;
    public ActorRemovedRenderer(GameActor a) {  this.id = a.id; }
    public void Render()
    {

    }
}
public class RoundStartRenderer : IGameEventRenderer
{
    public readonly int round;
    public RoundStartRenderer(int round) { this.round = round; }
    public void Render()
    {
    }
}
public class ActorActionsStartRenderer : IGameEventRenderer
{
    public readonly int actorId;
    public ActorActionsStartRenderer(GameActor a) {  this.actorId = a.id; }
    public void Render()
    {
    }
}
public class ActorActionsEndRenderer : IGameEventRenderer
{
    public readonly int actorId;
    public ActorActionsEndRenderer(GameActor a) {  this.actorId = a.id; }
    public void Render()
    {
    }
}
public class TargetSelectedRenderer : IGameEventRenderer
{
    public readonly int actorId;
    public readonly int targetId;
    public TargetSelectedRenderer(GameActor a)
    {
        actorId = a.id;
        targetId = a.Target == null ? GameActorState.NULL_ID : a.Target.id;
    }
    public void Render()
    {
    }
}
public class AttackRenderer : IGameEventRenderer
{
    public readonly int actorId;
    public readonly int targetId;
    public AttackRenderer(GameActor attacker, GameActor target)
    {
        actorId = attacker.id;
        targetId = target.id;
    }
    public void Render()
    { }
}
public class HealthChangeRenderer : IGameEventRenderer
{
    public readonly int actorId;
    public readonly float baseHealth;
    public readonly float before;
    public readonly float after;
    public HealthChangeRenderer(GameActor a, float before, float after)
    {
        actorId = a.id;
        baseHealth = a.baseHealth;
        this.before = before;
        this.after = after;
    }
    public void Render()
    { }
}
public class DeathRenderer : IGameEventRenderer
{
    public readonly int actorId;
    public DeathRenderer(GameActor a) { actorId = a.id; }
    public void Render()
    { }
}