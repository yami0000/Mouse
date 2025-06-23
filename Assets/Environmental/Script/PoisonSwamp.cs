using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSwamp : MonoBehaviour
{
    private EntityStats stats => GetComponentInParent<EntityStats>();

    private HashSet<Player> playersInSwamp = new HashSet<Player>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        PlayerStats playerStats = collision.GetComponent<PlayerStats>();

        if (player != null && playerStats != null && !playersInSwamp.Contains(player))
        {
            playersInSwamp.Add(player);
            stats.DoDamage(playerStats); // Initial damage
            StartCoroutine(DamageOverTime(player, playerStats));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null && playersInSwamp.Contains(player))
        {
            playersInSwamp.Remove(player);
        }
    }

    private IEnumerator DamageOverTime(Player player, PlayerStats playerStats)
    {
        while (playersInSwamp.Contains(player))
        {
            yield return new WaitForSeconds(1.5f);

            if (player != null && playerStats != null && playersInSwamp.Contains(player))
            {
                stats.DoDamage(playerStats);
            }
        }
    }
}
