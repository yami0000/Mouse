
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    private Effects fx;
     public int absorbedDAMAGE; 

    [Header("Damage")]
    public Stats FirePower;
    public Stats Damage;
    public Stats CriticalChance;
    public Stats CriticalDamage;

    [Header("Defensive Stats")]
    public Stats Vitality;
    public Stats MaxHP;
    public Stats Armor;
    public Stats FireResistance;
    public Stats FrostResistance;
    public Stats PoisonResistance;
    public Stats LightingResistance;

    [Header("Element")]
    public Stats FireDamage;
    public Stats FrostDamage;
    public Stats PoisonDamage;
    public Stats LightningDamage;

    [Header("support power")]
    public Stats HealPower;


    private bool isIgnited;
    private bool isFrosted;
    private bool isShocked;
    private bool isPoisoned;

    public int CurrentHP;

    private float fireMeter = 0;
    private float frostMeter = 0;
    private float shockMeter = 0;
    private float poisonMeter = 0;

    [Header("Element Threshold")]
    [SerializeField] private float Fire_THRESHOLD;
    [SerializeField] private float Frost_THRESHOLD;
    [SerializeField] private float Lighting_THRESHOLD;
    [SerializeField] private float Poison_THRESHOLD;

    private float ignitedTimer; 
    private float frostTimer;
    private float poisonTimer;
    private float lightningTimer;


    private float igniteDamageCD = 0.2f;
    private float igniteDamageTimer ;
    private int igniteDamage;


    private bool Dead;

    protected virtual void Start()
    {
        CurrentHP = GetMaxHealth();
        CriticalDamage.SetDefaultValue(150);
        fx = GetComponent<Effects>();
        Dead = false;   
    }

    protected virtual void Update()
    {
         

        ignitedTimer -= Time.deltaTime;
        igniteDamageTimer -= Time.deltaTime;

        if (ignitedTimer < 0)isIgnited = false;
        if (poisonTimer < 0)isPoisoned = false;
        if (frostTimer < 0) isFrosted = false;
        if (lightningTimer < 0) isShocked = false;
      

        if (igniteDamageTimer < 0 && isIgnited)
        {
            //Debug.Log(gameObject.name + " is taking " + igniteDamage + " burning damage");

            CurrentHP -= igniteDamage;

            if (CurrentHP <= 0)
            { Die(); 
              isIgnited = false;
              Dead = true;
            }

            igniteDamageTimer = igniteDamageCD;
        }

        if (lightningTimer > 0 && isShocked)
        { }


    }

   


    public virtual int DoDamage(EntityStats _targetStats, float damageMultiplier = 1.0f)
    {
       
            int baseDamage = Damage.GetValue() + FirePower.GetValue();

            baseDamage = (int)(baseDamage * damageMultiplier);

            if (CanCritical())
                baseDamage = CalculateCrit(baseDamage);

            int absorbedDamage;
            int finalDamage = ArmorSystem(_targetStats, baseDamage, out absorbedDamage);

            Debug.Log($"Final Damage: {finalDamage}, Absorbed: {absorbedDamage}");

            _targetStats.TakeDamage(finalDamage);
            DoElementDamage(_targetStats);

            return finalDamage;
       

    }

    public virtual void Heal(EntityStats _targetStats) 
    {
    int totalHeal = HealPower.GetValue() * 10;

        _targetStats.HealAmount(totalHeal);
    
    
    }

    public virtual void HealEntity(int HealAmount) 
    {
    
    }

    public virtual void Revive()
    {
        Dead = false;
        CurrentHP = GetMaxHealth();

        // clear lingering status effects so the player doesn't revive still on fire/poisoned
        isIgnited = false;
        isFrosted = false;
        isShocked = false;
        isPoisoned = false;

        fireMeter = 0;
        frostMeter = 0;
        shockMeter = 0;
        poisonMeter = 0;
    }

    public virtual void DoElementDamage(EntityStats _targetStats)
    {
        int _fireDamage = FireDamage.GetValue();
        int _frostDamage = FrostDamage.GetValue();
        int _lightingDamage = LightningDamage.GetValue();
        int _poisionDamage = PoisonDamage.GetValue();

         
        int totalElementDamage = CheckTargetFireResistance(_targetStats, _fireDamage)+CheckTargetFrostResistance(_targetStats, _frostDamage) + CheckTargetLightingResistance(_targetStats, _lightingDamage) + CheckTargetPoisonResistance(_targetStats,_poisionDamage);


        _targetStats.TakeDamage(totalElementDamage);
        _targetStats.ApplyElementalBuildup(_targetStats,FireDamage.GetValue(), FrostDamage.GetValue(), LightningDamage.GetValue(), PoisonDamage.GetValue());

         
        _targetStats.SetIgniteDamage(Mathf.RoundToInt(_fireDamage * .1f));
    }


    #region ELEMENTS
    public void ApplyElementalBuildup(EntityStats _targetStats, int fire, int frost, int lightning, int poison)
    {

        fire = CheckTargetFireResistance( _targetStats, fire);
        frost = CheckTargetFrostResistance( _targetStats, frost);
        lightning = CheckTargetLightingResistance( _targetStats, lightning);
        poison = CheckTargetPoisonResistance( _targetStats, poison);

        fireMeter += fire;
        frostMeter += frost;
        shockMeter += lightning;
        poisonMeter += poison;

      
        if (fireMeter >= Fire_THRESHOLD)
        {
            //Debug.Log("Fire! " +fireMeter);
            isIgnited = true;
            ignitedTimer = 4;
            fx.IgniteFxFor(4);
            fireMeter = 0;
        }
        if (frostMeter >= Frost_THRESHOLD)
        {
           // Debug.Log("Freeze! "+ frostMeter);
            isFrosted = true;
            frostTimer = 3;
            fx.FreezeFxFor(3);

            GetComponent<Entity>().SlowEntityBy(.5f,3);

            frostMeter = 0;
        }

        if (shockMeter >= Lighting_THRESHOLD)
        {
            //Debug.Log("Shock! "+ shockMeter);
            isShocked = true;
            lightningTimer = 0.5f;
            fx.ShockFxFor(.5f);
            GetComponent<Entity>().SlowEntityBy(1, .5f);
            shockMeter = 0;

        }
        if (poisonMeter >= Poison_THRESHOLD)
        {
           // Debug.Log("Poison! " + poisonMeter);
            isPoisoned = true;
            poisonTimer = 5;
            fx.PoisonFxFor(5);
            poisonMeter = 0;

        }
    }
    #endregion

    public void SetIgniteDamage(int _damage) => igniteDamage = _damage;

    public virtual void TakeDamage(int _damage)
    {
        CurrentHP -= _damage;

        


        if (CurrentHP <= 0 && !Dead)
        {
            Dead = true;
            Die(); 
        }
    }

    public virtual void HealAmount(int totalHeal) 
    {
       CurrentHP += totalHeal;

        if(CurrentHP  >= MaxHP.GetValue())
            CurrentHP = MaxHP.GetValue();
    
    }
    protected virtual void Die()
    {
       
    }

    #region ElementResistance
    private static int CheckTargetFireResistance(EntityStats _targetStats, int FireElementDamage)
    {
        FireElementDamage -= _targetStats.FireResistance.GetValue() ;
        FireElementDamage = Mathf.Clamp(FireElementDamage, 0, int.MaxValue);
        return FireElementDamage;
    }

    private static int CheckTargetFrostResistance(EntityStats _targetStats, int FrostElementDamage)
    {
        FrostElementDamage -= _targetStats.FrostResistance.GetValue();
        FrostElementDamage = Mathf.Clamp(FrostElementDamage, 0, int.MaxValue);
        return FrostElementDamage;
    }

    private static int CheckTargetLightingResistance(EntityStats _targetStats, int LightingElementDamage)
    {
        LightingElementDamage -= _targetStats.LightingResistance.GetValue();
        LightingElementDamage = Mathf.Clamp(LightingElementDamage, 0, int.MaxValue);
        return LightingElementDamage;
    }

    private static int CheckTargetPoisonResistance(EntityStats _targetStats, int PoisonElementDamage)
    {
        PoisonElementDamage -= _targetStats.PoisonResistance.GetValue();
        PoisonElementDamage = Mathf.Clamp(PoisonElementDamage, 0, int.MaxValue);
        return PoisonElementDamage;
    }

    #endregion

    public int ArmorSystem(EntityStats _targetStats, int incomingDamage, out int absorbedDamage)
    {
        int armorValue = _targetStats.Armor.GetValue();

        // If poisoned, armor effectiveness reduced
        if (_targetStats.isPoisoned)
            armorValue = Mathf.RoundToInt(armorValue * 0.7f);

        absorbedDamage = Mathf.Clamp(armorValue, 0, incomingDamage);
        int finalDamage = Mathf.Clamp(incomingDamage - absorbedDamage, 0, int.MaxValue);

        _targetStats.AbsorbedDamage(absorbedDamage);

        return finalDamage;
    } 

    public void AbsorbedDamage(int absorbedDamage) 
    {
        absorbedDAMAGE = absorbedDamage;


    }

    public bool CanCritical()
    {
        int totalCriticalChance = CriticalChance.GetValue()  ;

        if (Random.Range(0, 100) <= totalCriticalChance)
        {
            return true;
        }

        return false;

    }
    public int CalculateCrit(int _damage)
    {
      float totalCritPower = (CriticalDamage.GetValue()) * 0.01f; 
     // Debug.Log("total crit power" + totalCritPower);

      float critDamage = _damage * totalCritPower;
       // Debug.Log("crit damage is"+ critDamage);

      return Mathf.RoundToInt(critDamage);
    }

    public int GetMaxHealth()
    {
        return MaxHP.GetValue() + Vitality.GetValue() * 5; 

    }
}
