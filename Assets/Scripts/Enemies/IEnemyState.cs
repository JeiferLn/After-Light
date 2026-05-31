public interface IEnemyState
{
    void Enter(EnemyStateMachine sm);
    void Update(EnemyStateMachine sm, float dt);
    void Exit(EnemyStateMachine sm);
}