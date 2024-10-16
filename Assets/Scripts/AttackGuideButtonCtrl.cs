using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackGuideButtonCtrl : MonoBehaviour
{
    public Character characterMovement;
    void Start()
    {
        
    }

    void Update()
    {
        if(characterMovement.ifAttack)
        {
            gameObject.GetComponent<Button>().interactable = true;
        }
        else gameObject.GetComponent<Button>().interactable = false;
    }
}
