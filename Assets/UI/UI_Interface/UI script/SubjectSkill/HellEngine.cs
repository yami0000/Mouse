/// <summary>
/// The Hell Engine active skill and the Hellfire mechanism that feeds on it. One use lasts until the
/// temporary health it gave is gone, and only then does the cooldown start. Hellfire rewards a use that
/// ended with no damage taken; any damage during a use loses the bonus built up so far.
/// </summary>
public class HellEngine
{
    private readonly PlayerStats stats;
    private int cleanUses;
    private bool clean;

    public bool IsRunning { get; private set; }

    public HellEngine(PlayerStats _stats)
    {
        stats = _stats;
        stats.OnDamageTaken += () => clean = false;
        stats.OnTemporaryHealthGone += End;
    }

    public void Trigger()
    {
        ListedActiveSkill skill = SkillList.FindActive(ActiveSkillId.HellEngine);
        float percent = skill.hellEngineHealth;

        if (Skill.Instance.Hellfire)
            percent += cleanUses * SkillList.FindMechanism(SkillMechanism.Hellfire).hellfireBonusHealth;

        float amount = percent * 0.01f * stats.GetMaxHealth();
        if (amount <= 0f) return;

        IsRunning = true;
        clean = true;
        stats.GiveTemporaryHealth(amount, skill.hellEngineSeconds);
    }

    private void End()
    {
        if (!IsRunning) return;

        IsRunning = false;
        Skill.Instance.StartCooldown(ActiveSkillId.HellEngine);

        if (clean && Skill.Instance.Hellfire)
        {
            Skill.Instance.ReduceCooldown(ActiveSkillId.HellEngine, SkillList.FindMechanism(SkillMechanism.Hellfire).hellfireCooldownReduction);
            cleanUses++;
        }
        else
        {
            cleanUses = 0;
        }
    }
}
