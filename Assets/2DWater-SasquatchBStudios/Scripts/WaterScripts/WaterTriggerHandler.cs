using UnityEngine;

public class WaterTriggerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _waterMask;
    [SerializeField] private GameObject _splashParticles;

    private EdgeCollider2D _edgeColl;

    private InteractableWater _water;

    private void Awake()
    {
        _edgeColl = GetComponent<EdgeCollider2D>();
        _water = GetComponent<InteractableWater>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if((_waterMask.value & (1<<collision.gameObject.layer)) >0)
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            if(rb!= null)
            {
                //spawn particles
                if(_splashParticles != null)
                {
                    Vector2 localPos = gameObject.transform.localPosition;
                    Vector2 hitObjectPos = collision.transform.position;
                    Bounds hitObjectBounds = collision.bounds;

                    Vector3 spawnPos = Vector3.zero;
                    if(collision.transform.position.y >= _edgeColl.points[1].y + _edgeColl.offset.y +localPos.y)
                    {
                        //hit from aboce
                        spawnPos = hitObjectPos - new Vector2(0f, hitObjectBounds.extents.y);
                    }
                    else
                    {
                        //hit from below
                        spawnPos = hitObjectPos - new Vector2(0f, hitObjectBounds.extents.y);
                    }

                    Instantiate(_splashParticles, spawnPos, Quaternion.identity);
                }

                //clamp splash ponit to a MAX velocity
                int multiplier = 1;
                if (rb.linearVelocity.y < 0)
                {
                    multiplier = -1;
                }
                else
                {
                    multiplier = 1;
                }
                float vel = rb.linearVelocity.y * _water.ForceMultiplier;
                vel = Mathf.Clamp(Mathf.Abs(vel),0f,_water.MaxForce);
                vel *= multiplier;

                _water.Splash(collision, vel);
            }
        }
    }
}
