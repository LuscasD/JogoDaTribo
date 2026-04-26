using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int playerHealth;
    public int scrap;





    private void Awake()
    {
        // Se já existe uma instância e não é essa, destrói
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Define como instância única
        Instance = this;

        // Faz o objeto persistir entre cenas
        DontDestroyOnLoad(gameObject);
    }





}