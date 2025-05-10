using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Text_PlayerStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Health;
    [SerializeField] private TextMeshProUGUI FirePower;
    [SerializeField] private TextMeshProUGUI Armor;
    [SerializeField] private TextMeshProUGUI CritChance;
    [SerializeField] private TextMeshProUGUI CritDMG;   
    public void Update()
    {
        PlayerStats player = PlayerManager.Instance.player.GetComponent<PlayerStats>();
        Health.   text = player.CurrentHP.ToString() + "/" + player.GetMaxHealth();
        FirePower.text = player.FirePower.GetValue().ToString();
        Armor.    text = player.Armor.GetValue().ToString();
        CritChance.text= player.CriticalChance.GetValue().ToString() + "%";
        CritDMG  .text = player.CriticalDamage.GetValue().ToString() + "%";
    }
}
