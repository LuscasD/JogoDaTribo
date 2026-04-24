using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TestSceneButton : MonoBehaviour
{

    MapManager mapManager;
    Button button;

    void Start()
    {
        mapManager = MapManager.Instance;
        button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        mapManager.GoToScene("WorldMap");
    }
}
