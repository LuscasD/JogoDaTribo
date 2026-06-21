using UnityEngine;

/// <summary>
/// Bala dos inimigos (raycast, sem Rigidbody/Collider — só o visual).
/// Atinge o PlayerHealth (ao contrário da Projectile do jogador, que atinge Enemy).
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _dir;
    private float _speed;
    private int _damage;
    private float _knockback;
    private LayerMask _hitMask;
    private float _life;
    private Transform _ignoreRoot;

    public void Launch(Vector3 direction, float speed, int damage, float knockback,
                       LayerMask hitMask, float lifeTime, Transform ignoreRoot)
    {
        _dir = direction.normalized;
        _speed = speed;
        _damage = damage;
        _knockback = knockback;
        _hitMask = hitMask;
        _life = lifeTime;
        _ignoreRoot = ignoreRoot;
    }

    private void Update()
    {
        float step = _speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, _dir, out RaycastHit hit, step, _hitMask, QueryTriggerInteraction.Ignore))
        {
            // ignora o próprio inimigo que atirou
            if (_ignoreRoot != null && hit.transform.IsChildOf(_ignoreRoot))
            {
                transform.position += _dir * step;
            }
            else
            {
                var ph = hit.collider.GetComponent<PlayerHealth>() ?? hit.collider.GetComponentInParent<PlayerHealth>();
                if (ph != null)
                {
                    Vector3 kb = _dir; kb.y = 0f; kb.Normalize();
                    ph.TakeDamage(_damage, kb * _knockback);
                }
                Destroy(gameObject); // some ao bater no player OU na parede
                return;
            }
        }
        else
        {
            transform.position += _dir * step;
        }

        _life -= Time.deltaTime;
        if (_life <= 0f) Destroy(gameObject);
    }
}
