using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ScrapTextScript : MonoBehaviour
{

    public TextMeshProUGUI  scrapText;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        scrapText.SetText("Scrap: " + GameManager.Instance.scrap); 
    }
}
