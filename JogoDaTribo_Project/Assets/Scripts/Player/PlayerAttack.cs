using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Rotação do Torso")]
    [SerializeField] private Transform body;
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float bodyHeightOffset = 1f; // altura do torso no mundo

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        RotateBodyTowardsMouse();
    }

    private void RotateBodyTowardsMouse()
    {
        if (body == null || cam == null) return;

        // Plano horizontal na altura do torso
        Plane bodyPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y + bodyHeightOffset, 0f));

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (bodyPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            // Direção do torso até o ponto do mouse
            Vector3 direction = worldPoint - body.position;
            direction.y = 0f; // mantém a rotação só no plano horizontal

            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            body.rotation = Quaternion.Slerp(body.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }
}





//-----------------------------------------------ANTIGO ATAQUE-----------------------------------//
//[SerializeField] private float attackRange = 2f;
//[SerializeField] private float attackCooldown = 0.4f;
//[SerializeField] private float movementLockDuration = 0.2f;

//[Header("Referências (Opcional)")]
//[SerializeField] private Animator animator;

//private float lastAttackTime = -999f;
//private int damageBonus = 0;
//private PlayerMovment movement;

//private static readonly int AnimAtacou = Animator.StringToHash("Atacou");

//private void Awake()
//{
//    if (animator == null)
//        animator = GetComponentInChildren<Animator>();
//    movement = GetComponent<PlayerMovment>();
//}

//private void Update()
//{
//    if (animator != null)
//        animator.SetBool(AnimAtacou, false);

//    if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
//        PerformAttack();
//}

//private void PerformAttack()
//{
//    lastAttackTime = Time.time;

//    movement?.LockMovement(movementLockDuration);

//    if (animator != null)
//        animator.SetBool(AnimAtacou, true);

//    Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
//    Collider[] hits = Physics.OverlapSphere(center, attackRange * 0.5f);

//    foreach (var hit in hits)
//    {
//        if (hit.gameObject == gameObject) continue;

//        var enemy = hit.GetComponent<Enemy>();
//        if (enemy == null)
//            enemy = hit.GetComponentInParent<Enemy>();

//        if (enemy != null)
//        {
//            Vector3 knockback = (hit.transform.position - transform.position).normalized * knockbackForce;
//            enemy.TakeDamage(attackDamage + damageBonus, knockback);
//        }
//    }
//}

//public void SetDamageBonus(int bonus)
//{
//    damageBonus = bonus;
//}

//private void OnDrawGizmos()
//{
//    Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
//    Gizmos.DrawSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange * 0.5f);
//}

