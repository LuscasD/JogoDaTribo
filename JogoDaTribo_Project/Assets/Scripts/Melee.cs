using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Melee : Enemy
{

    private NavMeshAgent nav;
    
    
    [SerializeField]
    private float stopDistance, dashForce;
    

    [SerializeField] Transform playerPos;
    [SerializeField] State currentState;

    //Helpers
    float timer;
    bool isAttacking = false;


    private enum State
    {
        Idle,
        Chasing,
        Attacking,
        TakingDamage
    }

    
    void Start()
    {
        //Nav Variables
        nav = GetComponent<NavMeshAgent>();
        nav.speed = speed;
        nav.stoppingDistance = stopDistance;


        currentState = State.Idle;

    
    }



    // Update is called once per frame
    void Update()
    {
        //StateMachine



        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Chasing:
                HandleChasing(playerPos);
                break;
            case State.Attacking:
                HandleAttacking();
                break;
            case State.TakingDamage:
                HandleTakingDamage();
                break;
        }

    }

    private void HandleIdle()
    {
        
        Transform target = null;     

        Collider[] hits = Physics.OverlapSphere(transform.position, vision_radius);


        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                target = hit.transform;
                break;
            }
        }

        
        if (target != null)
        {
            nav.SetDestination(target.position);
            currentState = State.Chasing;
        }
    }

    private void HandleChasing(Transform target)
    {
    
        
        timer += Time.deltaTime;
        print(nav.remainingDistance);
      

        if (timer > 0.2f)
        {
           
            timer = 0f;
        }

        if (nav.remainingDistance <= stopDistance)
        {
            currentState = State.Attacking;
            timer = 0f;
            nav.ResetPath();
            return;
        }

    }

  
    private void HandleAttacking()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackingCouroutine());
            isAttacking = true;
        }


    }


    private IEnumerator AttackingCouroutine()
    {
        // Rotaciona até alinhar
        yield return RotateUntilAligned(playerPos, 640f);

        Attack();
        yield return new WaitForSeconds(0.5f);

        yield return RotateUntilAligned(playerPos, 640f);

        Attack();

        yield return new WaitForSeconds(2f);
        //Ataca e Ataca
        //Recuperação
        //Verifica se o player ainda está no alcance, se sim, ataca novamente, se não, volta para chasing

        isAttacking = false;
    }



  

    private void HandleTakingDamage()
    {
        // Lógica do estado TakingDamage
    }





    //ATAQUE

    private void Attack()
    {
        Dash();
        //Colisões
    }

    private void Dash()
    {
        var dir = transform.forward;
        rb.AddForce(dir * dashForce, ForceMode.Impulse);
    }



    //ROTAÇÃO PARA O ATAQUE
    private IEnumerator RotateUntilAligned(Transform target, float rotationSpeed, float tolerance = 1f)
    {
        while (!IsRotationComplete(target, tolerance))
        {
            RotateTowards(target, rotationSpeed);
            yield return null;
        }
    }

    private void RotateTowards(Transform target, float rotationSpeed)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Mantém a rotação apenas no eixo Y

        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            lookRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private bool IsRotationComplete(Transform target, float tolerance = 1f)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction == Vector3.zero) return true;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        float angle = Quaternion.Angle(transform.rotation, lookRotation);
        return angle < tolerance;
    }





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, vision_radius);
    }


}
