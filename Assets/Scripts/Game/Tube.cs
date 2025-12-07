using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tube : MonoBehaviour
{
    public bool isSelected = false;
    public bool isCompleted = false;
    [Header("Water Layers")]
    public Transform waterRoot;
    public List<GameObject> layers = new List<GameObject>();
    [Header("Tube Cap")]
    public Transform cap;    
    public Vector3 capClosedPos;
    public float capDropHeight = 1.0f;

    bool capIsClosed = false;

    [HideInInspector] public Transform tubeTransform;

    void Awake()
    {
        tubeTransform = this.transform;
        cap.gameObject.SetActive(false);
    }


    void OnMouseDown()
    {
        if (isCompleted)
            return;
        _ = LevelManager.Instance.OnTubeClickedAsync(this);
    }
    // public void SetSelected(bool value)
    // {
    //     isSelected = value;

    //     if (value)
    //         transform.localPosition += new Vector3(0, 0.2f, 0);
    //     else
    //         transform.localPosition -= new Vector3(0, 0.2f, 0);
    // }
    public void SetSelected(bool value)
    {
        isSelected = value;
        tubeTransform.DOKill();

        Vector3 targetPos = value
            ? transform.localPosition + new Vector3(0, 0.2f, 0)
            : transform.localPosition - new Vector3(0, 0.2f, 0);
        tubeTransform.DOLocalMove(targetPos, 0.2f).SetEase(Ease.OutSine);
    }
    public bool IsCompleted()
    {
        if (layers.Count != LevelManager.Instance.layerPerTube)
            return false;

        Color c = layers[0].GetComponent<WaterLayer>().fill.color;

        for (int i = 1; i < layers.Count; i++)
        {
            if (layers[i].GetComponent<WaterLayer>().fill.color != c)
                return false;
        }
        if (layers.Count == LevelManager.Instance.layerPerTube)
        { 
            AudioManager.Instance.PlayFull();
            CloseCap();
        }
        return true;
    }
    public void CloseCap()
    {
        if (capIsClosed)
            return;
        capIsClosed = true;
        cap.gameObject.SetActive(true);
        Vector3 startPos = capClosedPos + new Vector3(0, capDropHeight, 0);
        cap.localPosition = startPos;
        cap.DOLocalMove(capClosedPos, 0.25f)
        .SetEase(Ease.OutBounce);
        cap.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
        AudioManager.Instance.PlayClose();
    }

}
