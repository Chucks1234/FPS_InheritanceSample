using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Save_Load_Script : MonoBehaviour
{
    public void OnSave()
    {
        FPSController player = FindObjectOfType<FPSController>();
        player.SavePlayerData();
    }
    public void OnLoad()
    {
        FPSController player = FindObjectOfType<FPSController>();
        player.LoadPlayerData();
    }
}
