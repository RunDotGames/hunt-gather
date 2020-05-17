
using UnityEngine;
using System;
using System.Collections.Generic;
using DragonBones;
using System.Linq;


public class Villager : MonoBehaviour, CombatTarget {

  public List<VillagerArmature> armatures;
  
  public delegate void VillagerEvent(Villager target);
  public event Action<CombatTarget> OnDeath;

  private Dictionary<VillagerType, Agent> agents;
  
  private Dictionary<VillagerType, VillagerArmature> armaturesByType;
  private Func<bool> onHunger;
  private SeasonalTimeRange feedRange;
  private VillagerConfig config;
  private VillagerType type;
  private bool isDead = false;
  
  public void Init(VillagerConfig config, VillagerType type){
    armaturesByType = armatures.Aggregate(new Dictionary<VillagerType, VillagerArmature>(), (agg, item) => {
      agg[item.type] = item;
      item.Init();
      return agg;
    });
    this.config = config;
    agents = new Dictionary<VillagerType, Agent>();
    var commonConfig = new AgentConfigCommon(){
      arrivalDistance = config.arrivalDistance,
      restRange = new SeasonalTimeRange(config.restRange),
      transform = this.transform,
      speed = config.speed,
    };
    agents[VillagerType.Gatherer] = new GathererAgent(commonConfig);
    agents[VillagerType.Gatherer].onEvent += armaturesByType[VillagerType.Gatherer].animator.HandleEvent;
    
    agents[VillagerType.Hunter] = new CombatAgent(commonConfig, "villager", this);
    agents[VillagerType.Hunter].onEvent += armaturesByType[VillagerType.Hunter].animator.HandleEvent;
    
    agents[VillagerType.Builder] = new BuilderAgent(commonConfig);
    agents[VillagerType.Builder].onEvent += armaturesByType[VillagerType.Builder].animator.HandleEvent;
    SetVillagerType(type);
  }

  public void ProvideFeeder(Func<bool> feeder, Func<bool> storageCheck, Action store, RandomFloatRange feedRange, Func<Vector2, Vector2> storeLocation){
    this.onHunger = feeder;
    this.feedRange = new SeasonalTimeRange(feedRange).Resume();
    ((GathererAgent)agents[VillagerType.Gatherer]).ProvideFeeder(storageCheck, store, storeLocation);
  }

  public void ProvideHarvester(Func<Vector2, HarvestTarget> getTarget){
    ((GathererAgent)agents[VillagerType.Gatherer]).ProvideHarvester(getTarget);
  }

  public void ProvideCombatant(Combatant combatant){
    ((CombatAgent)agents[VillagerType.Hunter]).ProvideCombatant(combatant);
  }

  public VillagerType GetVillagerType(){
    return type;
  }

  public void SetVillagerType(VillagerType type){
    agents[this.type].Release();
    armaturesByType[this.type].Hide();
    this.type = type;
    agents[this.type].Resume();
    armaturesByType[this.type].Show();
  } 

  private void UpdateHunger(){
    if(feedRange == null || !feedRange.Update(SeasonTask.Eat)) {
      return;
    }
    var fed = onHunger?.Invoke() ?? true;
    if(!fed){
      Kill("starvation");
    }
  }

  public void Update(){
    if(isDead || agents == null){
      return;
    }
    agents[type]?.Update();
    UpdateHunger();
  }

  public void Kill(string reason){
    Debug.Log("villager died bc " + reason);
    isDead = true;
    agents[type].Release();
    OnDeath?.Invoke(this);
    HandleEntityEvent(EntityEventType.Die);
    StartCoroutine(PlayDeath());
  }

  private IEnumerator<YieldInstruction> PlayDeath(){
    yield return new WaitForSeconds(2);
    GameObject.Destroy(gameObject);
  }

  public int GetAvailability(){
    return agents[type].GetAvailability();
  }

  public MonoBehaviour GetBehaviour(){
    return this;
  }

  public void HandleEntityEvent(EntityEventType type){
    armaturesByType[this.type].animator.HandleEvent(type);
  }
}