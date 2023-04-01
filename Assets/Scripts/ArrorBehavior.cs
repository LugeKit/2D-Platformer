using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrorBehavior : MonoBehaviour
{
    public Vector2 Speed;
    public float LifeTime;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.velocity = Speed;
    }

    private void Update()
    {
        LifeTime -= Time.deltaTime;
        if (LifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

}
