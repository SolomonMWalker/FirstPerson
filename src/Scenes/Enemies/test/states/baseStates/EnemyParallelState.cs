using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyParallelState: ParallelState
{
    public Grunt Grunt { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        foreach (var child in GetChildren())
        {
            switch (child)
            {
                case EnemyAtomicState aState:
                    aState.Grunt = Grunt;
                    break;
                case EnemyCompoundState cState:
                    cState.Grunt = Grunt;
                    break;
                case EnemyParallelState pState:
                    pState.Grunt = Grunt;
                    break;
            }
        }
    }
}