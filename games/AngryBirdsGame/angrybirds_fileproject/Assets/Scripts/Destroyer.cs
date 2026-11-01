using UnityEngine;

public class Destroyer : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        string tag = col.gameObject.tag;
        if (tag == "Enemy" || tag == "Bird" || tag == "Obstacle")
        {
            Destroy(col.gameObject);
        }
    }
}