using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    [SerializeField]
    private GameObject currentBubble;
    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("ClickDown");
            if (currentBubble == null)
                currentBubble = Instantiate(bubblePrefab, mousePos, Quaternion.identity);

            Rigidbody2D rb2d = currentBubble.GetComponent<Rigidbody2D>();
            rb2d.isKinematic = true;
        }
        //hold down mouse to make a bigger bubble
        if(Input.GetMouseButton(0))
        {
            Debug.Log("HoldClick");
            currentBubble.transform.position = mousePos;
            if(currentBubble.transform.localScale.x <1)
            {
                Vector2 scaleValue = Vector2.one * Time.deltaTime * .1f;
                Vector3 scaleUp = new Vector3(scaleValue.x, scaleValue.y, 0);
                currentBubble.transform.localScale += scaleUp;
            }
            else
            {
                currentBubble.transform.localScale = Vector3.one;
            }
        }
        //release to fire off a bubble
        if(Input.GetMouseButtonUp(0))
        {
            Debug.Log("ClickUp");
            Rigidbody2D rb2d = currentBubble.GetComponent<Rigidbody2D>();
            rb2d.isKinematic = false;
            currentBubble = null;
        }
    }
}
