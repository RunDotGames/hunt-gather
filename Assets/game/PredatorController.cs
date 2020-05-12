
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
  public float prowlPace;
  public float arrivalDistance;
  public float attackDistance;
  public RandomFloatRange idleTime;
  public float spotDistance;
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
  private Dictionary<ScreenSide, SideConfig> spawnConfig = new Dictionary<ScreenSide, SideConfig>();

  private float nextSpawn;
  private List<Predator> predators = new List<Predator>();
  
  public void Start(){
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

  
private void Spawn(){
    var village = GameObject.FindObjectOfType<VillageController>();
    var predator = GameObject.Instantiate(config.prefab).GetComponent<Predator>();
    var screenSide = (ScreenSide)UnityEngine.Random.Range(0, SideCount);
    var sideConfig = spawnConfig[screenSide];
    var x = UnityEngine.Random.Range(0, Screen.width);
    var y = UnityEngine.Random.Range(0, Screen.height);
    var spawnPoint = new Vector2(sideConfig.comp.x * x, sideConfig.comp.y * y) + sideConfig.nudge*config.spawnOutset + sideConfig.basis;
    var position = (Vector2)Camera.main.ScreenToWorldPoint(spawnPoint);
    predator.transform.position = new Vector3(position.x, position.y, 0);
    predators.Add(predator);
    predator.Init(new CombatAgentConfig(){
      arrivalDistance=config.arrivalDistance,
      idleRange=config.idleTime,
      prowlPace = config.prowlPace,
      attackPace = config.attackPace,
      spotDistance=config.spotDistance,
      transform = predator.transform,
      targetProvider = village.getNearestVillager,
      attackDistance = config.attackDistance,
    });
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