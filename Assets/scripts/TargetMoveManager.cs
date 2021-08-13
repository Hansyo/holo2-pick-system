using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public class TargetMoveManager : MonoBehaviour
{
    public float timeOut;

    private int i = 0;

    private Vector3[] targetPosition = {
        new Vector3(2f, 0f, 2f),
        new Vector3(-2f, 0f, -2f),
        new Vector3(-2f, 0f, 2f),
        new Vector3(2f, 0f, -2f),
    };


    void Start() {
        StartCoroutine( FuncCoroutine() );
    }

    IEnumerator FuncCoroutine() {
        while(true){
            TargetPositionUpdate();
            yield return new WaitForSeconds(timeOut);
        }
    }

    public void TargetPositionUpdate()
    {
        if (i >= targetPosition.Length)
        {
            i = 0;
        }
        this.gameObject.transform.position = targetPosition[i];
        i++;
    }
}
