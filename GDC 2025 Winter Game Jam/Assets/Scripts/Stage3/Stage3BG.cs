using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    public float scrollSpeedX = 0f;
    public float scrollSpeedY = 0f;

    private float spriteWidth;
    private float spriteHeight;
    

    void Start()
    {
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        transform.Translate(Vector3.down * scrollSpeedY * Time.deltaTime);

        if (transform.position.y < -spriteHeight)
        {
            transform.position += new Vector3(0, spriteHeight * 2f, 0);
        }

        transform.Translate(Vector3.left * scrollSpeedX * Time.deltaTime);

        if (transform.position.x < -spriteWidth)
        {
            transform.position += new Vector3(spriteWidth * 2f, 0, 0);
        }
    }
}
