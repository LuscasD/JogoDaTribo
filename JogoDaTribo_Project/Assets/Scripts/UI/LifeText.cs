using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LifeText : MonoBehaviour
{
    public TextMeshProUGUI lifeText;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance != null)
            lifeText.SetText("Life: " + GameManager.Instance.playerHealth);
    }
}
