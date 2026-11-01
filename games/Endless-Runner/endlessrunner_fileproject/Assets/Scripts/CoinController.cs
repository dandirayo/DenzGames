using UnityEngine;

// FITUR: koin/collectible untuk skor bonus.
// Taruh di GameObject koin dengan Collider2D "Is Trigger" = true.
[RequireComponent(typeof(Collider2D))]
public class CoinController : MonoBehaviour
{
    public string playerTag = "Player";
    public int bonusScore = 10;

    [Header("Optional FX")]
    public AudioClip pickupSound;
    public GameObject pickupVfx;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        ScoreController scoreController = other.GetComponent<CharacterMoveController>()?.score;
        if (scoreController != null)
        {
            scoreController.AddBonusScore(bonusScore);
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        if (pickupVfx != null)
        {
            Instantiate(pickupVfx, transform.position, Quaternion.identity);
        }

        // Catatan: kalau koin ini juga dipakai lewat object pooling milik
        // TerrainGeneratorController, ganti baris di bawah jadi SetActive(false)
        // alih-alih Destroy, supaya konsisten dengan sistem pool yang sudah ada.
        gameObject.SetActive(false);
    }
}
