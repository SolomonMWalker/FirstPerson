using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyAtomicState: AtomicState
{
    public Grunt Grunt { get; set; }
}