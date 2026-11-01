using UnityEngine;

// FITUR: rintangan aktif yang harus dilompati (bukan cuma jurang).
// Taruh di GameObject rintangan dengan Collider2D "Is Trigger" = true,
// layer/tag "Player" dicek lewat CompareTag di bawah.
[RequireComponent(typeof(Collider2D))]
public class ObstacleController : MonoBehaviour
{
    public string playerTag = "Player";

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        CharacterMoveController character = other.GetComponent<CharacterMoveController>();
        if (character != null)
        {
            character.HitObstacle();
        }
    }
}
