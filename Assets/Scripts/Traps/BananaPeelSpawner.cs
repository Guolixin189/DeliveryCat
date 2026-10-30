using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// Keeps up to maxPeels banana peels on the field. Peels pop up at random
// times on random road tiles; when the player slips on one it disappears,
// and a replacement spawns somewhere else after a short random delay.
public class BananaPeelSpawner : MonoBehaviour
{
    public GameObject bananaPeelPrefab;
    public int maxPeels = 3;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 4f;
    public float minRespawnDelay = 3f;
    public float maxRespawnDelay = 6f;
    public float minPlayerDistance = 3f;
    public float minPeelDistance = 2f;

    private List<GameObject> active = new List<GameObject>();
    private List<Vector3> roadSpots = new List<Vector3>();
    private player.CatController cat;
    private float timer;

    void Start()
    {
        cat = FindFirstObjectByType<player.CatController>();
        CollectRoadSpots();
        timer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void CollectRoadSpots()
    {
        GameObject roads = GameObject.Find("roads");
        Tilemap tm = roads != null ? roads.GetComponent<Tilemap>() : null;
        if (tm == null)
            return;
        BoundsInt b = tm.cellBounds;
        for (int x = b.xMin; x < b.xMax; x++)
        {
            for (int y = b.yMin; y < b.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (tm.HasTile(cell))
                    roadSpots.Add(tm.GetCellCenterWorld(cell));
            }
        }
    }

    void Update()
    {
        if (cat == null)
            cat = FindFirstObjectByType<player.CatController>();
        int before = active.Count;
        active.RemoveAll(p => p == null);
        if (active.Count < before)
            timer = Random.Range(minRespawnDelay, maxRespawnDelay);
        if (active.Count >= maxPeels || bananaPeelPrefab == null)
            return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (SpawnOne())
                timer = Random.Range(minSpawnInterval, maxSpawnInterval);
            else
                timer = 1f;
        }
    }

    bool SpawnOne()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 spot = PickSpot();
            if (cat != null && Vector3.Distance(spot, cat.transform.position) < minPlayerDistance)
                continue;
            bool tooClose = false;
            foreach (GameObject p in active)
            {
                if (p != null && Vector3.Distance(spot, p.transform.position) < minPeelDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
                continue;
            GameObject peel = Instantiate(bananaPeelPrefab, spot, Quaternion.identity);
            active.Add(peel);
            return true;
        }
        return false;
    }

    Vector3 PickSpot()
    {
        if (roadSpots.Count > 0)
            return roadSpots[Random.Range(0, roadSpots.Count)];
        return new Vector3(Random.Range(-25f, 25f), Random.Range(-15f, 15f), 0f);
    }
}
