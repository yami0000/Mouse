using UnityEngine;
using UnityEngine.UI;

/// <summary>A Filled image over the health bar showing the player's temporary health. At 0 it can't be seen.</summary>
public class UI_TemporaryHealthBar : MonoBehaviour
{
    [SerializeField] private Image fill;

    private PlayerStats stats;

    private void Start() => stats = PlayerManager.Instance.player.GetComponent<PlayerStats>();

    private void Update()
    {
        if (stats != null)
            fill.fillAmount = stats.TemporaryHealthFraction;
    }
}
