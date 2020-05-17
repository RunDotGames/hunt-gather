
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PredatorControlConfig{
  public RandomFloatRange initialDelay;
  public RandomFloatRange spawnDelay;
  public GameObject prefab;
  public float spawnOutset;
  public float attackPace;
  public float attackDuration;
  public float prowlPace;
  public float arrivalDistance;
  public float attackDistance;
  public RandomFloatRange idleTime;
  public float spotDistance;
}

[Serializable]
public class HunterConfig {
  public float prowlPace;
  public float spotDistance;
  public float attackDistance;
  public float attackDuration;
}

public class PredatorController : MonoBehaviour {
  private enum ScreenSide { Left, Right, Top, Bottom }
  
  private class SideConfig {

    public Vector2 comp;
    public Vector2 basis;
    public Vector2 nudge;
  }
  private readonly int SideCount = Enum.GetNames(typeof(ScreenSide)).Length;

  public PredatorControlConfig config;
  public HunterConfig hunterConfig;
  private Dictionary<ScreenSide, SideConfig> spawnConfig = new Dictionary<ScreenSide, SideConfig>();

  private float nextSpawn;
  private List<Predator> predators = new List<Predator>();
  private VillageController village;

  public void Init(VillageController village){
    this.village = village;
    village.OnVillagerSpawned += HandleVillagerSpawned;
    spawnConfig.Add(ScreenSide.Top, new SideConfig{comp=Vector2.right, basis=Vector2.up*Screen.height, nudge=Vector2.up});
    spawnConfig.Add(ScreenSide.Bottom, new SideConfig{comp=Vector2.right, basis=Vector2.zero, nudge=Vector2.down});
    spawnConfig.Add(ScreenSide.Right, new SideConfig{comp=Vector2.up, basis=Vector2.right*Screen.width, nudge=Vector2.right});
    spawnConfig.Add(ScreenSide.Left, new SideConfig{comp=Vector2.up, basis=Vector2.zero, nudge=Vector2.left});
    nextSpawn = Time.time + config.initialDelay.GetRangeValue();
  }

  public void Update(){
    if(Time.time > nextSpawn){
      nextSpawn = Time.time + config.spawnDelay.GetRangeValue();
      Spawn();
    }
  }

  private void HandleVillagerSpawned(Villager villager){
    villager.ProvideCombatant(new Combatant(){
      attackDistance = hunterConfig.attackDistance,
      prowlSpeed = hunterConfig.prowlPace,
      spotDistance = hunterConfig.spotDistance,
      targetProvider = GetNearestPredator,
      attackDuration = hunterConfig.attackDuration,
    });
  }

  
  private void Spawn(){
    var predator = GameObject.Instantiate(config.prefab).GetComponent<Predator>();
    var screenSide = (ScreenSide)UnityEngine.Random.Range(0, SideCount);
    var sideConfig = spawnConfig[screenSide];
    var x = UnityEngine.Random.Range(0, Screen.width);
    var y = UnityEngine.Random.Range(0, Screen.height);
    var spawnPoint = new Vector2(sideConfig.comp.x * x, sideConfig.comp.y * y) + sideConfig.nudge*config.spawnOutset + sideConfig.basis;
    var position = (Vector2)Camera.main.ScreenToWorldPoint(spawnPoint);
    predator.transform.position = new Vector3(position.x, position.y, 0);
    predators.Add(predator);
    predator.Init(
      new Combatant(){
        attackDuration = config.attackDuration,
        attackDistance = config.attackDistance,
        prowlSpeed = config.prowlPace,
        spotDistance = config.spotDistance,
        targetProvider = village.getNearestVillager,
      },
      new AgentConfigCommon(){
        arrivalDistance = config.arrivalDistance,
        restRange = new SeasonalTimeRange(config.idleTime),
        speed = config.attackPace,
        transform = predator.transform,
      }
    );
    predator.OnDeath += HandlePredatorDeath;
  }

  private void HandlePredatorDeath(CombatTarget target){
    var predator = (Predator)target;
    predators.Remove(predator);
  }

  public Predator GetNearestPredator(Vector2 from) {
    return DistanceUtility.GetNearest(from, predators);
  }

}