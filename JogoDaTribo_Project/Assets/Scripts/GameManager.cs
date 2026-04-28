using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int playerHealth = 3;
    public int scrap = 0;
    public TextMeshProUGUI  scrapText;



    private void Awake()
    {
        // Se j� existe uma inst�ncia e n�o � essa, destr�i
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Define como inst�ncia �nica
        Instance = this;
        if(scrapText == null)
        {
        scrapText = GameObject.Find("ScrapText")?.GetComponent<TextMeshProUGUI>();
        }
        scrapText?.SetText("Scrap: " + scrap);

        // Faz o objeto persistir entre cenas
        DontDestroyOnLoad(gameObject);
    }

   void Update()
    {
      scrapText?.SetText("Scrap: " + scrap);  
    }





}