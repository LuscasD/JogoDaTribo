using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private LayerMask enemyLayer;

    private float lastAttackTime = -999f;

    private void Update()
    {
        RotateTowardsMouse();

        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
            PerformAttack();
    }

    private void RotateTowardsMouse()
    {
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 dir = hitPoint - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, attackRange * 0.5f, enemyLayer);

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector3 knockback = (hit.transform.position - transform.position).normalized * knockbackForce;
                enemy.TakeDamage(attackDamage, knockback);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
        Gizmos.DrawSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange * 0.5f);
    }
}
