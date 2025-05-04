
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    private Effects fx;
    
    [Header("Major Stats")]
    public Stats FirePower;
    public Stats Agility;
    public Stats Intelligence;
    public Stats Vitality;

    [Header("Damage")]
    public Stats Damage;
    public Stats CriticalChance;
    public Stats CriticalDamage;

    [Header("Defensive Stats")]
    public Stats MaxHP;
    public Stats Armor;
    public Stats Evasion;
    public Stats ElementResistance;

    [Header("Element")]
    public Stats FireDamage;
    public Stats FrostDamage;
    public Stats PoisonDamage;
    public Stats LightningDamage;

    public bool isIgnited;
    public bool isFrosted;
    public bool isShocked;
    public bool isPoisoned;

    public int CurrentHP;

    private float fireMeter = 0;
    private float frostMeter = 0;
    private float shockMeter = 0;
    private float poisonMeter = 0;

    // Threshold to trigger status effect
    private const float STATUS_THRESHOLD = 100f;

    private float ignitedTimer; 
    private float frostTimer;
    private float poisonTimer;
    private float lightningTimer;


    private float igniteDamageCD = 0.2f;
    private float igniteDamageTimer ;
    private int igniteDamage;

    protected virtual void Start()
    {
        CurrentHP = GetMaxHealth();
        CriticalDamage.SetDefaultValue(150);
        fx = GetComponent<Effects>();
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
            
            }

            igniteDamageTimer = igniteDamageCD;
        }

        if (lightningTimer > 0 && isShocked)
        { }


    }

    public virtual void DoDamage(EntityStats _targetStats)
    {
        if (Avoid(_targetStats))
            return;

        int totalDamage = Damage.GetValue() + FirePower.GetValue();

        if (CanCritical())
            totalDamage = CalculateCrit(totalDamage);

        totalDamage = ArmorSystem(_targetStats, totalDamage);

        _targetStats.TakeDamage(totalDamage);
        DoElementDamage(_targetStats);

    }

    public virtual void DoElementDamage(EntityStats _targetStats)
    {
        int _fireDamage = FireDamage.GetValue();
        int _frostDamage = FrostDamage.GetValue();
        int _lightingDamage = LightningDamage.GetValue();
        int _poisionDamage = PoisonDamage.GetValue();

        int totalElementDamage = _fireDamage + _frostDamage + _lightingDamage + _poisionDamage + Intelligence.GetValue();
        totalElementDamage = CheckTargetElementResistance(_targetStats, totalElementDamage);

        _targetStats.TakeDamage(totalElementDamage);
        _targetStats.ApplyElementalBuildup(_targetStats,FireDamage.GetValue(), FrostDamage.GetValue(), LightningDamage.GetValue(), PoisonDamage.GetValue());

         
        _targetStats.SetIgniteDamage(Mathf.RoundToInt(_fireDamage * .1f));
    }


    #region ELEMENTS
    public void ApplyElementalBuildup(EntityStats _targetStats, int fire, int frost, int lightning, int poison)
    {

        fire = CheckTargetElementResistance( _targetStats, fire);
        frost = CheckTargetElementResistance( _targetStats, frost);
        lightning = CheckTargetElementResistance( _targetStats, lightning);
        poison = CheckTargetElementResistance( _targetStats, poison);

        fireMeter += fire;
        frostMeter += frost;
        shockMeter += lightning;
        poisonMeter += poison;

      
        if (fireMeter >= STATUS_THRESHOLD)
        {
            //Debug.Log("Fire! " +fireMeter);
            isIgnited = true;
            ignitedTimer = 4;
            fx.IgniteFxFor(4);
            fireMeter = 0;
        }
        if (frostMeter >= STATUS_THRESHOLD)
        {
           // Debug.Log("Freeze! "+ frostMeter);
            isFrosted = true;
            frostTimer = 3;
            fx.FreezeFxFor(3);

            GetComponent<Entity>().SlowEntityBy(.5f,3);

            frostMeter = 0;
        }

        if (shockMeter >= STATUS_THRESHOLD)
        {
            //Debug.Log("Shock! "+ shockMeter);
            isShocked = true;
            lightningTimer = 0.5f;
            fx.ShockFxFor(.5f);
            GetComponent<Entity>().SlowEntityBy(1, .5f);
            shockMeter = 0;

        }
        if (poisonMeter >= STATUS_THRESHOLD)
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

        //Debug.Log(_damage); 


        if (CurrentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {

    }
    private static int CheckTargetElementResistance(EntityStats _targetStats, int totalElementDamage)
    {
        totalElementDamage -= _targetStats.ElementResistance.GetValue() + (_targetStats.Intelligence.GetValue() * 2);
        totalElementDamage = Mathf.Clamp(totalElementDamage, 0, int.MaxValue);
        return totalElementDamage;
    }
    private int ArmorSystem(EntityStats _targetStats, int totalDamage)
    {
        if (_targetStats.isPoisoned)
        {
            totalDamage -= Mathf.RoundToInt(_targetStats.Armor.GetValue() * 0.7f);
                
        }
        else
            totalDamage -= _targetStats.Armor.GetValue();

        totalDamage = Mathf.Clamp(totalDamage, 0, int.MaxValue);
        return totalDamage;
    }  //Reduce taken damage by armor
    private bool Avoid(EntityStats _targetStats)
    {
        int totalevasion = _targetStats.Evasion.GetValue() + _targetStats.Agility.GetValue();

        if (Random.Range(0, 100) < totalevasion)
        {

            return true;

        }
        return false;
    }//Chances to avoid damage.
    private bool CanCritical()
    {
        int totalCriticalChance = CriticalChance.GetValue() + Agility.GetValue();

        if (Random.Range(0, 100) <= totalCriticalChance)
        {
            return true;
        }

        return false;

    }
    private int CalculateCrit(int _damage)
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
