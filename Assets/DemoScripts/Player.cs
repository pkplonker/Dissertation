using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void Start()
    {
        SetCanMove(false);
    }

    public void SetCanMove(bool state)
    {
        GetComponent<StarterAssetsInputs>().cursorLocked = state;
        GetComponent<CharacterController>().enabled = state;
        GetComponent<ThirdPersonController>().enabled = state;
    }
}
