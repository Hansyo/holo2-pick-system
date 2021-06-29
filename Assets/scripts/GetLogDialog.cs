using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GetLogDialog : MonoBehaviour
{
    public TextMeshProUGUI logView;

    // Start is called before the first frame update
    void Start()
    {
        Application.logMessageReceived += logMessageReceived;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void logMessageReceived(string condition, string stackTrace, LogType type)
    {
        logView.text = condition;
        switch (type)
        {
            case LogType.Error:
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                break;
            case LogType.Log:
                break;
            case LogType.Exception:
                break;
        }
    }
}
