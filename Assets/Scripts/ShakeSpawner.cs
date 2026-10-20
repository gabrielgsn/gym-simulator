using UnityEngine;

public class ShakeSpawner : MonoBehaviour
{
    public GameObject shakePrefab;
    public Vector2[] spawnPoints;
    public float interval = 10f;
    public int maxActive = 3;

    float timer;

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
            return;

        timer += Time.deltaTime;
        if (timer < interval)
            return;

        timer = 0f;
        var active = GameObject.FindGameObjectsWithTag("Coletavel");
        if (active.Length >= maxActive)
            return;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            bool occupied = false;
            foreach (var s in active)
                if (Vector2.Distance(s.transform.position, point) < 0.5f)
                    occupied = true;

            if (!occupied)
            {
                Instantiate(shakePrefab, point, Quaternion.identity, transform);
                return;
            }
        }
    }
}
