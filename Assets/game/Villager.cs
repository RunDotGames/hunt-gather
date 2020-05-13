
using UnityEngine;
using System;
using System.Collections.Generic;

public class Villager : MonoBehaviour, CombatTarget {

  public SpriteRenderer sprite;

  private Dictionary<VillagerType, Agent> agents;
  private Dictionary<VillagerType, Color> colorMap = new Dictionary<VillagerType, Color>();
  private float nextHungry = -1;
  
  public delegate bool OnHunger();
  public delegate void VillagerEvent(Villager target);
  private OnHunger onHunger;
  private VillagerConfig config;

  public event Action<CombatTarget> OnDeath;

  private VillagerType type;
  private bool isDead = false;
  public void Init(VillagerConfig config, OnHunger onHunger, VillagerType type, Func<Vector2, CombatTarget> targetProvider){
    colorMap[VillagerType.Gatherer] = config.gatherColor;
    colorMap[VillagerType.Hunter] = config.hunterColor;
    colorMap[VillagerType.Builder] = config.builderColor;
    sprite.color = colorMap[type];
    this.type = type;
    this.config = config;
    this.onHunger = onHunger;
    agents = new Dictionary<VillagerType, Agent>();
    agents[VillagerType.Gatherer] = new GathererAgent(new GathererAgentConfig(){
      arrivalDistance = config.arrivalDistance,
      gatherTime = config.gatherTime,
      speed = config.speed,
      transform = this.transform,
      restRange = config.restRange,
    });
    agents[VillagerType.Hunter] = new CombatAgent(new CombatAgentConfig(){
      arrivalDistance = config.arrivalDistance,
      attackPace = config.attackPace,
      prowlPace = config.prowlPace,
      idleRange = config.restRange,
      spotDistance = config.spotDistance,
      transform = this.transform,
      targetProvider = targetProvider,
      attackDistance = config.attackDistance,
    }, "villager");
    agents[VillagerType.Builder] = new BuilderAgent(new BuilderConfig(){
      arrivalDistance = config.arrivalDistance,
      restRange = config.restRange,
      transform = transform,
      walkSpeed = config.speed,
      workRange = config.workRange,
    });
    UpdateHunger(false);
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
        var fed = onHunger();
        if(!fed){
          Kill("starvation");
        }
      }
      nextHungry = Time.time + config.hungerRange.GetRangeValue();
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