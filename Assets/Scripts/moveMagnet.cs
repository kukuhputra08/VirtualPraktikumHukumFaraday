using UnityEngine;

public class DragMagnet : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector2 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

    }

    void OnMouseDown()
    {
        isDragging = true;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, 0);
    }

    void OnMouseUp()
    {
        isDragging = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.MovePosition(lastPosition);
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 newPos = (Vector2)(mousePos + offset);
            rb.MovePosition(newPos);
            lastPosition = newPos;
        }
    }
}
