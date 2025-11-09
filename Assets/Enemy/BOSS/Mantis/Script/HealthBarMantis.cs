using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarMantis : MonoBehaviour
{
    public enum CreatureType { Mantis, Scorpion }
    public CreatureType creatureType;

    public Image fill;

    void Update()
    {
        float currentHealth = 0f;
        float maxHealth = 1f;

        switch (creatureType)
        {
            case CreatureType.Mantis:
                currentHealth = GM.Instance.GameManager.MantisHealth;
                maxHealth = GM.Instance.GameManager.MantisMaxHealth;
                break;

            case CreatureType.Scorpion:
                currentHealth = GM.Instance.GameManager.ScorpionHealth;
                maxHealth = GM.Instance.GameManager.ScorpionMaxHealth;
                break;
        }

        fill.fillAmount = currentHealth / maxHealth;
    }

}
