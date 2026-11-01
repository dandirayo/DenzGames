using UnityEngine;

// FITUR TEMA INDONESIA: "Polisi Tidur" (speed bump).
// Beda dari ObstacleController biasa: bukan instant game over, tapi
// memperlambat karakter sesaat kalau tidak sempat dilompati.
//
// Setup tanpa aset gambar (placeholder):
//   1. Buat GameObject baru, tambah SpriteRenderer dengan sprite bawaan
//      Unity "Sprites/Square" (Assets bawaan engine, tidak perlu import apa2)
//   2. Set warna jadi kuning-hitam (mis. #F2C230) supaya kebayang "road bump"
//   3. Scale pipih memanjang (mis. X=1, Y=0.15) supaya terlihat seperti gundukan
//   4. Tambah Collider2D (BoxCollider2D), centang "Is Trigger"
//   5. Attach script ini
[RequireComponent(typeof(Collider2D))]
public class SpeedBumpObstacle : MonoBehaviour
{
    public string playerTag = "Player";

    [Header("Efek Slow-Down")]
    public float slowDuration = 1.2f;
    [Range(0.1f, 0.9f)]
    public float slowFactor = 0.4f;

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
            character.ApplySlowDown(slowDuration, slowFactor);
        }
    }
}
