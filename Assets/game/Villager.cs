
using UnityEngine;
using System;
using System.Collections.Generic;
using DragonBones;

public class Villager : MonoBehaviour, CombatTarget {

  public List<EntityAnimatorConfig> animConfig;
  public SpriteRenderer sprite;
  public UnityArmatureComponent armature;
  
  public delegate void VillagerEvent(Villager target);
  public event Action<CombatTarget> OnDeath;

  private Dictionary<VillagerType, Agent> agents;
  private Dictionary<VillagerType, Color> colorMap = new Dictionary<VillagerType, Color>();
  private Func<bool> onHunger;
  private SeasonalTimeRange feedRange;
  private VillagerConfig config;
  private VillagerType type;
  private bool isDead = false;
  
  private EntityAnimator entityAnimator;


  public void Start(){
    if(armature!= null){
      this.entityAnimator = new EntityAnimator(animConfig, armature);
    }
  }
  public void Init(VillagerConfig config, VillagerType type){
    colorMap[VillagerType.Gatherer] = config.gatherColor;
    colorMap[VillagerType.Hunter] = config.hunterColor;
    colorMap[VillagerType.Builder] = config.builderColor;
    if(sprite != null){
      sprite.color = colorMap[type];
    }
    this.type = type;
    this.config = config;
    agents = new Dictionary<VillagerType, Agent>();
    var commonConfig = new AgentConfigCommon(){
      arrivalDistance = config.arrivalDistance,
      restRange = new SeasonalTimeRange(config.restRange),
      transform = this.transform,
      speed = config.speed,
    };
    agents[VillagerType.Gatherer] = new GathererAgent(commonConfig);
    agents[VillagerType.Hunter] = new CombatAgent(commonConfig, "villager");
    agents[VillagerType.Builder] = new BuilderAgent(commonConfig);
    agents[VillagerType.Gatherer].onEvent += HandleEntityEvent;
    agents[VillagerType.Hunter].onEvent += HandleEntityEvent;
    agents[VillagerType.Builder].onEvent += HandleEntityEvent;
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
    sprite.color = colorMap[type];
    agents[this.type].Release();
    this.type = type;
    agents[this.type].Resume();
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
    if(sprite){
      GameObject.Destroy(sprite);
    }
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
    entityAnimator?.HandleEvent(type);
  }
}