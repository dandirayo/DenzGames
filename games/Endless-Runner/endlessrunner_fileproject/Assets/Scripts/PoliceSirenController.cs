using UnityEngine;

// FITUR TEMA INDONESIA: lampu sirine merah-biru kedap-kedip untuk obstacle
// "Pos Razia Polisi" / mobil patroli. Murni efek warna, tidak perlu sprite
// custom - cukup taruh di child kecil (kotak/lingkaran) di atas obstacle.
//
// Setup tanpa aset gambar (placeholder):
//   1. Di prefab obstacle "Pos Razia" (pakai ObstacleController untuk collision-nya),
//      tambah child GameObject kecil di bagian atas
//   2. Tambah SpriteRenderer, sprite bawaan Unity "Sprites/Square" atau lingkaran kecil
//   3. Attach script ini ke child tsb
public class PoliceSirenController : MonoBehaviour
{
    public Color colorA = Color.red;
    public Color colorB = Color.blue;
    [Tooltip("Kedipan per detik")]
    public float blinkSpeed = 6f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // toggle warna solid tiap setengah periode, biar kesan "sirine",
        // bukan fade - jadi tidak pakai Lerp/PingPong yang mulus
        bool useColorA = Mathf.FloorToInt(Time.time * blinkSpeed) % 2 == 0;
        sr.color = useColorA ? colorA : colorB;
    }
}
