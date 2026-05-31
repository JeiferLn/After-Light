using UnityEngine;
using System.Collections;

public class AttackState : EnemyStateBase
{
    private float _attackTimer;
    private bool _isAttacking;

    public override void Enter(EnemyStateMachine sm)
    {
        _attackTimer = 0f;
        _isAttacking = false;
        sm.Agent.isStopped = true;
        // sm.Animator?.SetTrigger("Attack"); // Hook para animación
        Debug.Log($"[{sm.name}] 🗡️ Entró en modo Ataque");
    }

    public override void Update(EnemyStateMachine sm, float dt)
    {
        // 1. Si el jugador sale del rango (con margen de 30% para evitar flickering), vuelve a Chase
        Vector3 pPos = sm.Player != null ? sm.Player.position : GameManager.Instance.PlayerPosition;
        float escapeDistSqr = (sm.Config.attackRange * 1.3f) * (sm.Config.attackRange * 1.3f);

        if ((sm.transform.position - pPos).sqrMagnitude > escapeDistSqr)
        {
            Transition(sm, EnemyState.Chase);
            return;
        }

        // 2. Loop de ataque: controlado por timer en Update
        _attackTimer += dt;
        if (_attackTimer >= sm.Config.attackCooldown && !_isAttacking)
        {
            _attackTimer = 0f;
            PerformAttack(sm);
        }
    }

    public override void Exit(EnemyStateMachine sm)
    {
        sm.Agent.isStopped = false;
        Debug.Log($"[{sm.name}] 🏃 Salió de modo Ataque");
    }

    private void PerformAttack(EnemyStateMachine sm)
    {
        _isAttacking = true;

        // 🎬 Hook: Aquí dispararás animaciones, sonidos o partículas
        // sm.Animator?.SetTrigger("Attack");
        // AudioController.Play("EnemyHit");

        // 💥 Aplicar daño (desacoplado en el futuro vía eventos)
        Debug.Log($"[{sm.name}] Golpea al jugador: {sm.Config.attackDamage} daño");
        // GameManager.Instance.PlayerHealth?.TakeDamage(sm.Config.attackDamage);

        // Bloquea nuevos ataques hasta que termine la "animación" de golpe
        sm.StartCoroutine(AttackLock(sm));
    }

    private IEnumerator AttackLock(EnemyStateMachine sm)
    {
        // Tiempo que dura el windup + impacto + recovery
        yield return new WaitForSeconds(0.4f);
        _isAttacking = false;
    }
}