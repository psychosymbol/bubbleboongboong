using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class BubbleSpawner : MonoBehaviour
{
    public static BubbleSpawner instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public GameObject bubblePrefab;
    private GameObject currentBubble;
    Vector3 lastMousePos;
    Vector3 mouseDelta { get { return Input.mousePosition - lastMousePos; } }

    public Transform bubbleParent;

    public List<GameObject> smallBubbles = new List<GameObject>();

    public float maxBubbleSize = 2f;
    public float minBubbleSize = .5f;
    public float chargeUpSpeed = 0.5f;
    public float chargeDownSpeed = 0.2f;
    public float charge_forceMultiplier = 10f;
    public float small_forceMultiplier = 5f;
    public float bubbleMassMultiplier = 0.1f;
    public float smallSpawnDistance = 10; // px unit
    [HideInInspector]
    public float currentSpawnDistance = 0f;

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("ClickDown");
            currentSpawnDistance = 0f;
            CreateChargeBubble(mousePos);
        }

        //hold down mouse to make a bigger bubble
        if (Input.GetMouseButton(0))
        {
            //Debug.Log("HoldClick");
            UpdateChargeBubble(mousePos);
        }

        //release to fire off a bubble
        if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("ClickUp");
            ReleaseChargeBubble(mousePos);
        }

        lastMousePos = Input.mousePosition;
    }

    void CreateChargeBubble(Vector3 mousePos)
    {
        if (currentBubble != null) return;

        currentBubble = Instantiate(bubblePrefab, mousePos, Quaternion.identity, bubbleParent);

        Rigidbody2D rb2d = currentBubble.GetComponent<Rigidbody2D>();
        rb2d.isKinematic = true;
        lastMousePos = Input.mousePosition;

        Collider2D bubbleCollider = rb2d.GetComponent<Collider2D>();
        bubbleCollider.enabled = false;
    }

    void ReleaseChargeBubble(Vector3 mousePos)
    {
        var currentBubbleScale = currentBubble.transform.localScale.x;
        if (minBubbleSize > currentBubbleScale)
        {
            Destroy(currentBubble);
            return;
        }

        Rigidbody2D rb2d = currentBubble.GetComponent<Rigidbody2D>();
        Collider2D bubbleCollider = rb2d.GetComponent<Collider2D>();

        rb2d.isKinematic = false;
        rb2d.mass = currentBubbleScale * bubbleMassMultiplier;
        rb2d.AddForce(Vector2.up * charge_forceMultiplier, ForceMode2D.Impulse);
        bubbleCollider.enabled = true;
        currentBubble = null;
    }

    void UpdateChargeBubble(Vector3 mousePos)
    {
        if (currentBubble == null) return;
        var currentBubbleScale = currentBubble.transform.localScale.x;
        currentBubble.transform.position = mousePos;

        currentSpawnDistance += mouseDelta.magnitude;
        if (currentSpawnDistance > smallSpawnDistance)
        {
            // Vector2 scaleValue = Vector2.one * chargeDownSpeed;
            // currentBubble.transform.localScale -= (Vector3)scaleValue;

            currentBubbleScale -= chargeDownSpeed;
            if (currentBubbleScale < minBubbleSize)
            {
                currentBubble.transform.localScale = new Vector3(minBubbleSize, minBubbleSize, 1);
            }
            else
            {
                currentBubble.transform.localScale = new Vector3(currentBubbleScale, currentBubbleScale, 1);
            }

            ReleaseSmallBubble(mousePos);
            currentSpawnDistance -= smallSpawnDistance;
        }
        else
        {
            if (currentBubbleScale < maxBubbleSize)
            {
                Vector2 scaleValue = chargeUpSpeed * Time.deltaTime * Vector2.one;
                currentBubble.transform.localScale += (Vector3)scaleValue;
            }
            else
            {
                currentBubble.transform.localScale = Vector3.one * maxBubbleSize;
            }
        }
    }

    void ReleaseSmallBubble(Vector3 mousePos)
    {
        var smallbubble = Instantiate(bubblePrefab, mousePos, Quaternion.identity, bubbleParent);
        Rigidbody2D rb2d = smallbubble.GetComponent<Rigidbody2D>();
        rb2d.mass = 0.5f * bubbleMassMultiplier;
        rb2d.AddForce(Vector2.up * small_forceMultiplier, ForceMode2D.Impulse);
        smallBubbles.Add(smallbubble);
    }

}
