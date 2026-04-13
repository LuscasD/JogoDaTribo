using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Info Base")]
    [SerializeField] protected int life;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int MaxLife;
    [SerializeField] protected float speed;
    [SerializeField] protected float vision_radius;



    // Start is called before the first frame update
    void Start()
    {
        life = MaxLife;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
