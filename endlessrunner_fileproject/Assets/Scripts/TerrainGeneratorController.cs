using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    [Header("Templates")]
    public List<TerrainTemplateController> terrainTemplates;
    public float terrainTemplateWidth;

    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;

    [Header("Generator Area")]
    public Camera gameCamera;
    public float areaStartOffSet;
    public float areaEndOffSet;
    public bool recyclePassedTerrain = false;

    // EFISIENSI: terrain selalu di-generate & dihapus berurutan (FIFO),
    // jadi Queue pas dan Dequeue() O(1) — sebelumnya List + pencarian
    // posisi float (foreach + "==") yang O(n) dan rawan mismatch presisi.
    private Queue<GameObject> spawnedTerrain;

    private float lastGeneratedPositionX;
    private float lastRemovedPositionX;

    private const float debugLineHeight = 10.0f;

    // EFISIENSI: pool per key pakai Stack (push/pop O(1)),
    // sebelumnya List + Remove(x) yang O(n) tiap ambil dari pool.
    private Dictionary<string, Stack<GameObject>> pool;


    private void Start()
    {
        // init pool
        pool = new Dictionary<string, Stack<GameObject>>();

        spawnedTerrain = new Queue<GameObject>();

        lastGeneratedPositionX = GetHorizontalPositionStart();
        lastRemovedPositionX = lastGeneratedPositionX - terrainTemplateWidth;

        foreach (TerrainTemplateController terrain in earlyTerrainTemplates)
        {
            GenerateTerrain(lastGeneratedPositionX, terrain);
            lastGeneratedPositionX += terrainTemplateWidth; //generate
        }

        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth; //generate

        }
    }

    private void Update()
    {
        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth; //generate
        }
        while (recyclePassedTerrain && lastRemovedPositionX + terrainTemplateWidth < GetHorizontalPositionStart())
        {
            lastRemovedPositionX += terrainTemplateWidth; //generate
            RemoveTerrain();

        }
    }

    private float GetHorizontalPositionStart()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffSet;
    }

    private float GetHorizontalPositionEnd()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffSet;
    }

    private void GenerateTerrain(float posX, TerrainTemplateController forceTerrain = null)
    {
        GameObject newTerrain = null;
        if(forceTerrain == null)
        {
             newTerrain = GenerateFromPool(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);
        }
        else
        {
            newTerrain = GenerateFromPool(forceTerrain.gameObject, transform);
        }

        newTerrain.transform.position = new Vector2(posX, 0f);
        spawnedTerrain.Enqueue(newTerrain);
    }

    // Terrain selalu dihapus dalam urutan yang sama dengan generate-nya,
    // jadi tidak perlu cari berdasarkan posisi lagi — cukup Dequeue().
    private void RemoveTerrain()
    {
        if (spawnedTerrain.Count == 0) return;

        GameObject terrainToRemove = spawnedTerrain.Dequeue();
        ReturnToPool(terrainToRemove);
    }
    //pool function
    private GameObject GenerateFromPool(GameObject item, Transform parent)
    {
        if(pool.ContainsKey(item.name))
        {
            //check if available in pool
            if(pool[item.name].Count > 0)
            {
                GameObject newItemFromPool = pool[item.name].Pop();
                newItemFromPool.SetActive(true);
                return newItemFromPool;
            }
        }
        else
        {
            // if item list not define -> create new
            pool.Add(item.name, new Stack<GameObject>());
        }

        GameObject newItem = Instantiate(item, parent);
        newItem.name = item.name;
        return newItem;
    }
    private void ReturnToPool(GameObject item)
    {
        if(!pool.ContainsKey(item.name))
        {
            Debug.LogError("INVALID POOL ITEM?!");
            return;
        }
        pool[item.name].Push(item);
        item.SetActive(false);
    }
    //debug


    private void OnDrawGizmos()
    {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition + Vector3.up * debugLineHeight / 2,
            areaStartPosition + Vector3.down * debugLineHeight / 2,
            Color.red);
        Debug.DrawLine(areaEndPosition + Vector3.up * debugLineHeight / 2,
            areaEndPosition + Vector3.down * debugLineHeight / 2,
            Color.red);
    }
}
