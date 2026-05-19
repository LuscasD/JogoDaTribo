using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ScrapDrop : MonoBehaviour
{
    Rigidbody rb;
    Transform playerTransform;
    bool isFollowingPlayer = false;
    bool spawning = true;
    [SerializeField]
    float pickDistance, speed, maxSpeed;

    


    // Start is called before the first frame update
    void Start()
    {    
        rb = GetComponent<Rigidbody>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(Spawning());
        Vector3 randomDir = new Vector3(
        Random.Range(-1f, 1f),
        Random.Range(0.5f, 1f), // leve impulso pra cima
        Random.Range(-1f, 1f)
        ).normalized;

        rb.AddForce(randomDir * 3f, ForceMode.Impulse);

    }

    // Update is called once per frame
    void Update()
    {
        CheckDistance();
        FollowPlayer(Vector3.Distance(gameObject.transform.position, playerTransform.position));


    }



     void CheckDistance()
    {
        if (spawning) return;
        if (isFollowingPlayer)return;

        float dist = Vector3.Distance(gameObject.transform.position, playerTransform.position);

        if ( dist < pickDistance) isFollowingPlayer = true;
       
    }

    void FollowPlayer(float distance)
    {
        if (!isFollowingPlayer) return;
        Vector3 direction = (playerTransform.position - gameObject.transform.position).normalized;
        rb.velocity = Vector3.ClampMagnitude(direction * (speed / distance), maxSpeed);
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.scrap += 1;
            Destroy(gameObject);
        }
    }

   


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, pickDistance);
    }


    public IEnumerator Spawning()
    {
        yield return new WaitForSeconds(0.4f);
        spawning = false;
    }



}
