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
        Health.   text = "HEALTH       " + player.CurrentHP.ToString();
        FirePower.text = "FIREPOWER    " + player.FirePower.GetValue().ToString();
        Armor.    text = "ARMOR        " + player.Armor.GetValue().ToString();
        CritChance.text= "CRIT CHANCE  " + player.CriticalChance.GetValue().ToString();
        CritDMG  .text = "CRIT DAMAGE  " + player.CriticalDamage.GetValue().ToString();
    }
}
