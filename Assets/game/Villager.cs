
using UnityEngine;
using System;
using System.Collections.Generic;

public class Villager : MonoBehaviour, CombatTarget {

  public SpriteRenderer sprite;
  
  public delegate void VillagerEvent(Villager target);
  public event Action<CombatTarget> OnDeath;

  private Dictionary<VillagerType, Agent> agents;
  private Dictionary<VillagerType, Color> colorMap = new Dictionary<VillagerType, Color>();
  private float nextHungry = -1;
  private Func<bool> onHunger;
  private RandomFloatRange feedRange;
  private VillagerConfig config;
  private VillagerType type;
  private bool isDead = false;
  
  public void Init(VillagerConfig config, VillagerType type){
    colorMap[VillagerType.Gatherer] = config.gatherColor;
    colorMap[VillagerType.Hunter] = config.hunterColor;
    colorMap[VillagerType.Builder] = config.builderColor;
    sprite.color = colorMap[type];
    this.type = type;
    this.config = config;
    agents = new Dictionary<VillagerType, Agent>();
    var commonConfig = new AgentConfigCommon(){
      arrivalDistance = config.arrivalDistance,
      restRange = config.restRange,
      transform = this.transform,
      speed = config.speed,
    };
    agents[VillagerType.Gatherer] = new GathererAgent(commonConfig);
    agents[VillagerType.Hunter] = new CombatAgent(commonConfig, "villager");
    agents[VillagerType.Builder] = new BuilderAgent(commonConfig);
    UpdateHunger(false);
  }

  public void ProvideFeeder(Func<bool> feeder, Func<bool> storageCheck, Action store, RandomFloatRange feedRange, Func<Vector2, Vector2> storeLocation){
    this.onHunger = feeder;
    this.feedRange = feedRange;
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
    sprite.color = colorMap[type];
    agents[this.type].Release();
    this.type = type;
    agents[this.type].Resume();
  } 

  private void UpdateHunger(bool eat){
    if(nextHungry < 0 || nextHungry < Time.time) {
      if(eat){
        var fed = onHunger?.Invoke() ?? true;
        if(!fed){
          Kill("starvation");
        }
      }
      nextHungry = Time.time + feedRange?.GetRangeValue() ?? 0;
    }
  }

  public void Update(){
    if(isDead || agents == null){
      return;
    }
    agents[type]?.Update();
    UpdateHunger(true);
  }

  public void Kill(string reason){
    Debug.Log("villager died bc " + reason);
    isDead = true;
    agents[type].Release();
    OnDeath?.Invoke(this);
    GameObject.Destroy(gameObject);
  }

  public int GetAvailability(){
    return agents[type].GetAvailability();
  }

  public MonoBehaviour GetBehaviour(){
    return this;
  }
}