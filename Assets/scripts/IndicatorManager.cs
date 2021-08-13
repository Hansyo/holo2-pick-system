using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject = default;
    [SerializeField]
    private GameObject indicatorObject = default;
    [SerializeField]
    private float forwardOffset = 1.5f;
    [SerializeField]
    private float heightOffset = -0.1f;
    private bool showIndicator = false;

    private void Start()
    {
        showIndicator = indicatorObject.activeInHierarchy;
    }

    void Update()
    {
        if (showIndicator)
        {
            DirectionUpdate();
        }
    }

    public void ToggleIndicator()
    {
        showIndicator = !showIndicator;
        if (showIndicator)
        {
            Debug.Log("Indicator ON");
        }
        else
        {
            Debug.Log("Indicator OFF");
        }
        SetIndicator();
    }

    private void SetIndicator()
    {
        if (targetObject == null)
        {
            showIndicator = false;
        }

        indicatorObject.SetActive(showIndicator);
    }

    private void DirectionUpdate()
    {
        if (targetObject == null)
        {
            return;
        }

        Vector3 indicatorPosition;
        Vector3 indicatorDirection;
        Quaternion indicatorRotation;

        indicatorPosition = Camera.main.gameObject.transform.position + Vector3.Scale(Camera.main.gameObject.transform.forward, new Vector3(1,0,1) * forwardOffset);
        indicatorPosition.y += heightOffset;

        indicatorDirection = (targetObject.transform.position - Camera.main.gameObject.transform.position).normalized;
        indicatorDirection.y = 0;

        indicatorRotation = Quaternion.LookRotation(indicatorDirection, Vector3.up);

        indicatorObject.transform.position = indicatorPosition;
        indicatorObject.transform.rotation = Quaternion.Lerp(indicatorObject.transform.rotation, indicatorRotation, 0.05f);
    }
}
