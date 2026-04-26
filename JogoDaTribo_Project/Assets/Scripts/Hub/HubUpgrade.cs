using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HubPrincipal : MonoBehaviour
{
    [Header("Interação")]
    float interactRange = 8f;
    [SerializeField] private int scrapCost = 10; // defina o custo depois

    [Header("Referências")]
    [SerializeField] private TextMeshProUGUI InteractText;
    [SerializeField] private GameObject InteractTextObject; // objeto do texto no mundo

    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject costTextObject;



    private PlayerMovment player;
    private bool playerInRange;

    void Start()
    {
        player = FindObjectOfType<PlayerMovment>();
        InteractTextObject.SetActive(false);
        costTextObject.SetActive(false);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerInRange = distance <= interactRange;

        if (playerInRange)
        {
            InteractTextObject.SetActive(true);
            costTextObject.SetActive(true);

            UpdateText();

            if (Input.GetKeyDown(KeyCode.F))
                TryInteract();
        }
        else
        {
            InteractTextObject.SetActive(false);
            costTextObject.SetActive(false);
        }
    }

    private void UpdateText()
    {
         bool hasScrap = GameManager.Instance.scrap >= scrapCost; // descomente quando tiver a variável


        costText.text = GameManager.Instance.scrap + "/" + scrapCost;
        if (hasScrap)
        { 
            InteractText.text = "<color=green>[F] Upgrade</color>";
        }
            else
            InteractText.text = "<color=red>[F] Upgrade</color>";
    }

    private void TryInteract()
    {

        //Não tem Scrap suficiente
        if (GameManager.Instance.scrap < scrapCost) return;


        //Tem Scrap suficiente
        GameManager.Instance.scrap -= scrapCost;
        Upgrade();


    }

    void Upgrade()
    {
        print("Vida Máxima: " + GameManager.Instance.playerHealth);
        GameManager.Instance.playerHealth += 20; // exemplo de upgrade, aumenta a vida máxima em 20
        print("Vida Máxima: " + GameManager.Instance.playerHealth);
        scrapCost += 20;
    }







}