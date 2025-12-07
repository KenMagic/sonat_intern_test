using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject tubePrefab;
    public GameObject waterLayerPrefab;

    [Header("Config")]
    public int tubeCount = 8;
    public int layerPerTube = 4;
    public int colorCount = 6;

    [Header("Layout")]
    public float spacing = 1.8f;
    public float startY = 0f;
    public float layerSpacing = 1.22f;
    public float layerBottomOffset = -0.65f;

    [Header("Pour Config")]
    public float pourMoveUp = 1.0f;
    public float pourTiltAngle = 45f;
    public float pourDuration = 0.5f;

    [Header("Level config")]
    public LevelConfig levelConfig;
    public int currentLevelIndex = 0;
    private LevelData currentLevelData;


    [HideInInspector] public bool isPouring = false;

    private List<Tube> spawnedTubes = new List<Tube>();
    Tube selectedTube = null;

    public static LevelManager Instance;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentLevelIndex = GameManager.Instance.currentLevel;
        LoadLevelData();
        SpawnTubes();
        FillRandomWater();
        CheckLose();
    }
    void LoadLevelData()
    {
        if (levelConfig != null && levelConfig.levels.Length > 0)
        {
            currentLevelData = levelConfig.levels[Mathf.Clamp(currentLevelIndex, 0, levelConfig.levels.Length - 1)];
            tubeCount = currentLevelData.tubeCount;
            colorCount = currentLevelData.colorCount;
            layerPerTube = currentLevelData.layerPerTube;
            spacing = currentLevelData.spacing;
            startY = currentLevelData.rowYOffset;
        }
    }

    // Spawn Tubes
    void SpawnTubes()
    {
        float centerOffset = (tubeCount - 1) * spacing / 2f;

        for (int i = 0; i < tubeCount; i++)
        {
            Vector3 pos = new Vector3(i * spacing - centerOffset, startY, 0);
            GameObject tubeObj = Instantiate(tubePrefab, pos, Quaternion.identity);

            Tube t = tubeObj.GetComponent<Tube>();
            spawnedTubes.Add(t);
        }
    }
    // Fill tubes với màu ngẫu nhiên
    void FillRandomWater()
    {
        // List màu
        List<Color> colors = new List<Color>()
        {
            new Color(1f, 0.3f, 0.3f),  // đỏ
            new Color(1f, 0.7f, 0.3f),  // cam
            new Color(1f, 1f, 0.3f),    // vàng
            new Color(0.3f, 1f, 0.3f),  // xanh lá
            new Color(0.3f, 0.7f, 1f),  // xanh dương
            new Color(1f, 0.3f, 1f)     // tím
        };

        // Shuffle list màu 
        List<int> pool = new List<int>();
        for (int c = 0; c < colorCount; c++)
            for (int i = 0; i < layerPerTube; i++)
                pool.Add(c);

        Shuffle(pool);

        int poolIndex = 0;

        for (int i = 0; i < spawnedTubes.Count; i++)
        {
            Tube tube = spawnedTubes[i];

            if (i < colorCount)
            {
                for (int l = 0; l < layerPerTube; l++)
                {
                    int colorID = pool[poolIndex++];
                    Color clr = colors[colorID];

                    SpawnWaterLayer(tube, clr, l);
                }
            }
        }
    }
    // Spawn các layer water
    void SpawnWaterLayer(Tube tube, Color color, int order)
    {
        WaterLayer w = Instantiate(waterLayerPrefab, tube.waterRoot)
                        .GetComponent<WaterLayer>();

        w.transform.localPosition = new Vector3(0, -0.65f + order * 1.22f, 0);

        w.SetColor(color);

        bool isTop = (order == layerPerTube - 1);
        bool isBottom = (order == 0);

        w.top.enabled = isTop;
        w.cover.enabled = isTop;
        w.bottom.enabled = isBottom;

        tube.layers.Add(w.gameObject);
    }
    // Shuffle list
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T tmp = list[i];
            list[i] = list[rand];
            list[rand] = tmp;
        }
    }
    // Handle tube click
    public async Task OnTubeClickedAsync(Tube tube)
    {
        if (isPouring) return;
        if (selectedTube == null)
        {
            selectedTube = tube;
            tube.SetSelected(true);
            AudioManager.Instance.PlayUp();
            return;
        }
        if (selectedTube == tube)
        {
            selectedTube.SetSelected(false);
            selectedTube = null;
            AudioManager.Instance.PlayDown();
            return;
        }
        // Pour(selectedTube, tube);
        await PourAll(selectedTube, tube);
        selectedTube.SetSelected(false);
        selectedTube = null;
    }
    // Pour nước
    void Pour(Tube from, Tube to)
    {
        if (!CanPourOnce(from, to))
            return;
        while (CanPourOnce(from, to))
        {
            PourOnce(from, to);
        }

        UpdateLayerPositions(from);
        UpdateLayerPositions(to);

        RefreshLayerEdges(from);
        RefreshLayerEdges(to);

        CheckCompleted(from);
        CheckCompleted(to);

        CheckLose();

        Debug.Log($"Pour from {from.name} to {to.name}");
    }
    //Check điều kiện có thể pour
    bool CanPourOnce(Tube from, Tube to)
    {
        if (from.layers.Count == 0)
            return false;
        if (to.layers.Count >= LevelManager.Instance.layerPerTube)
            return false;
        Color fromTop = from.layers[from.layers.Count - 1]
                            .GetComponent<WaterLayer>().fill.color;
        if (to.layers.Count == 0)
            return true;
        Color toTop = to.layers[to.layers.Count - 1]
                            .GetComponent<WaterLayer>().fill.color;
        if (fromTop != toTop)
            return false;
        int sameCount = 1;

        for (int i = from.layers.Count - 2; i >= 0; i--)
        {
            Color c = from.layers[i].GetComponent<WaterLayer>().fill.color;
            if (c == fromTop)
                sameCount++;
            else
                break;
        }
        int empty = LevelManager.Instance.layerPerTube - to.layers.Count;
        if (sameCount > empty)
            return false;

        return true;
    }
    // Pour một lớp nước
    void PourOnce(Tube from, Tube to)
    {
        GameObject layerObj = from.layers[from.layers.Count - 1];
        from.layers.RemoveAt(from.layers.Count - 1);

        layerObj.transform.SetParent(to.waterRoot);
        to.layers.Add(layerObj);
    }
    //Cập nhật vị trí layer trong ống
    void UpdateLayerPositions(Tube tube)
    {
        for (int i = 0; i < tube.layers.Count; i++)
        {
            tube.layers[i].transform.localPosition =
                new Vector3(0, layerBottomOffset + i * layerSpacing, 0);
        }
    }
    //Cập nhật hiển thị cạnh layer
    void RefreshLayerEdges(Tube tube)
    {
        if (tube.layers.Count == 0)
            return;

        for (int i = 0; i < tube.layers.Count; i++)
        {
            WaterLayer layer = tube.layers[i].GetComponent<WaterLayer>();

            layer.top.enabled = (i == tube.layers.Count - 1);
            layer.cover.enabled = (i == tube.layers.Count - 1);
            layer.bottom.enabled = (i == 0);
        }
    }
    //Kiểm tra ống đã hoàn thành
    void CheckCompleted(Tube tube)
    {
        if (tube.IsCompleted())
        {
            tube.isCompleted = true;
            tube.SetSelected(false);
        }
    }
    //Kiểm tra ống đã hoàn thành hoặc rỗng
    bool IsCompletedOrEmpty(Tube tube)
    {
        if (tube.layers.Count == 0) return true;
        return tube.IsCompleted();
    }

    //Kiểm tra thua
    public void CheckLose()
    {
        bool allDone = true;
        foreach (var t in spawnedTubes)
        {
            if (!IsCompletedOrEmpty(t))
            {
                allDone = false;
                break;
            }
        }
        if (allDone)
        {
            if (levelConfig != null && currentLevelIndex < levelConfig.levels.Length - 1)
            {
                GameManager.Instance.currentLevel++;
                GameManager.Instance.LoadLevel(GameManager.Instance.currentLevel);
            }
            else
            {
                GameManager.Instance.SetState(GameState.Win);
            }
            return;
        }

        for (int i = 0; i < spawnedTubes.Count; i++)
        {
            for (int j = 0; j < spawnedTubes.Count; j++)
            {
                if (i == j) continue;

                Tube from = spawnedTubes[i];
                Tube to = spawnedTubes[j];

                if (from.isCompleted) continue;
                if (to.isCompleted) continue;

                if (CanPourOnce(from, to))
                {
                    return;
                }
            }
        }
        GameManager.Instance.SetState(GameState.Lose);
    }
    
    async Task PourWithTween(Tube from, Tube to)
    {
        if (!CanPourOnce(from, to))
            return;

        // Lấy layer trên cùng
        GameObject layerObj = from.layers[from.layers.Count - 1];
        Color color = layerObj.GetComponent<WaterLayer>().fill.color;

        // Lưu vị trí ban đầu
        Vector3 startPos = from.tubeTransform.position;
        Quaternion startRot = from.tubeTransform.rotation;

        Vector3 targetPos = startPos + Vector3.up * pourMoveUp;
        Quaternion targetRot = Quaternion.Euler(0, 0, pourTiltAngle);

        float halfDur = pourDuration / 2f;

        // Di chuyển lên + nghiêng
        await DOTween.Sequence()
            .Join(from.tubeTransform.DOMove(targetPos, halfDur))
            .Join(from.tubeTransform.DORotateQuaternion(targetRot, halfDur))
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
        if (to.layers.Count > 0)
        {
            var topLayer = to.layers[to.layers.Count - 1].GetComponent<WaterLayer>();
            topLayer.cover.enabled = false;
        }
        // Pour nước
        float pourTime = halfDur;

        // Tạo layer mới trong ống to
        WaterLayer newLayer = GameObject.Instantiate(waterLayerPrefab, to.waterRoot)
                                       .GetComponent<WaterLayer>();
        newLayer.SetColor(color);
        newLayer.transform.localPosition =
            new Vector3(0, layerBottomOffset + to.layers.Count * layerSpacing, 0);
        newLayer.transform.localScale = new Vector3(1, 0, 1);

        to.layers.Add(newLayer.gameObject);

        Vector3 scaleFromStart = layerObj.transform.localScale;
        Vector3 scaleFromEnd = new Vector3(scaleFromStart.x, 0, scaleFromStart.z);

        // Scale water from và to
        await DOTween.Sequence()
            .Join(layerObj.transform.DOScale(scaleFromEnd, pourTime))
            .Join(newLayer.transform.DOScale(Vector3.one, pourTime))
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();

        // Xóa layer ở from
        from.layers.Remove(layerObj);
        GameObject.Destroy(layerObj);

        // Return ống về vị trí ban đầu
        await DOTween.Sequence()
            .Join(from.tubeTransform.DOMove(startPos, halfDur))
            .Join(from.tubeTransform.DORotateQuaternion(startRot, halfDur))
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();

        // Refresh edges
        RefreshLayerEdges(from);
        RefreshLayerEdges(to);
        CheckCompleted(from);
        CheckCompleted(to);
        CheckLose();
    }
    // Pour nước
    async Task PourAll(Tube from, Tube to)
    {
        isPouring = true;

        Vector3 originalPos = from.tubeTransform.position;
        await MoveTubeAboveUp(from, to, pourDuration / 2);
        while (CanPourOnce(from, to))
        {
            AudioManager.Instance.PlayPour();
            await PourWithTween(from, to);
        }
        await MoveTubeBack(from, originalPos, pourDuration / 2);
        isPouring = false;
    }
    // Move ống lên phía trên ống to
    async Task MoveTubeAboveUp(Tube from, Tube to, float duration = 0.3f)
    {
        Vector3 targetPos = to.tubeTransform.position + new Vector3(0, LevelManager.Instance.pourMoveUp, 0);

        await from.tubeTransform.DOMove(targetPos, duration)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }
    // Move ống về vị trí ban đầu
    async Task MoveTubeBack(Tube from, Vector3 originalPos, float duration = 0.3f)
    {
        await from.tubeTransform.DOMove(originalPos, duration)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }


}
