using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    public float radiusMult = 2f;
    public float forceMult = 5f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name == "BubblePopper")
        {
            BubblePop();
        }
    }

    public void BubblePop()
    {
        LayerMask mask = LayerMask.GetMask("WaterInteract");
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x * radiusMult, mask);
        List<Collider2D> affectedPushedObject = new List<Collider2D>();
        List<Collider2D> affectedBreakObject = new List<Collider2D>();
        foreach (Collider2D obj in objects)
        {
            if(obj.GetComponent<BubbleBehavior>() == null) affectedPushedObject.Add(obj);
            //else affectedBreakObject.Add(obj);
        }

        foreach (Collider2D obj in affectedPushedObject)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            rb.AddExplosionForce(transform.localScale.x * forceMult, transform.position, transform.localScale.x * radiusMult, 0, ForceMode2D.Impulse);
        }

        //foreach(Collider2D obj in affectedBreakObject)
        //{
        //    BubbleBehavior bubbleBehavior = obj.GetComponent<BubbleBehavior>();
        //    bubbleBehavior.BubblePop();
        //}

        Destroy(gameObject);
    }
}
