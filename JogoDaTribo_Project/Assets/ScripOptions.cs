using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ScripOptions : MonoBehaviour
{
  
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private GameObject menuPanel;   // o painel que abre/fecha
    [SerializeField] private bool pausarJogo = true; // congela o jogo enquanto aberto

    private bool aberto;

    private void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        aberto = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    public void Toggle()
    {
        aberto = !aberto;
        if (menuPanel != null) menuPanel.SetActive(aberto);
        if (pausarJogo) Time.timeScale = aberto ? 0f : 1f;
    }

    public void Fechar()
    {
        aberto = false;
        if (menuPanel != null) menuPanel.SetActive(false);
        if (pausarJogo) Time.timeScale = 1f;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f; // volta o tempo ao normal antes de trocar de cena
        SceneManager.LoadScene(menuScene);
    }
}
