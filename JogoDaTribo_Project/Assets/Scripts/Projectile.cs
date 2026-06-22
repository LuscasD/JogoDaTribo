using UnityEngine;

/// <summary>
/// Bala simples por raycast (não atravessa inimigos em alta velocidade).
/// Não precisa de Rigidbody nem Collider no prefab — só o visual (mesh/trail).
/// </summary>
public class Projectile : MonoBehaviour
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
            if (_ignoreRoot != null && hit.transform.IsChildOf(_ignoreRoot))
            {
                transform.position += _dir * step;
            }
            else
            {
                var enemy = hit.collider.GetComponent<Enemy>() ?? hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    Vector3 kb = _dir; kb.y = 0f; kb.Normalize();
                    enemy.TakeDamage(_damage, kb * _knockback);
                }
                Destroy(gameObject);
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