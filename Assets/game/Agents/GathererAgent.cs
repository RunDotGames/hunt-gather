
using UnityEngine;
using System.Collections.Generic;

public class GathererAgentConfig {
  public float speed;
  public Transform transform;
  public float arrivalDistance;
  public float gatherTime;
  public RandomFloatRange restRange;
}

public class GathererAgent: Agent {
  private static Dictionary<GathererState, int> statePriority = new Dictionary<GathererState, int>(){
    {GathererState.Idle, 0},
    {GathererState.Rest, 100},
    {GathererState.GoToFood, 200},
    {GathererState.GatherFood, 300},
    {GathererState.GoToHut, 0}
  };
  
  delegate void AgentUpdate();

  private enum GathererState {
    Idle, GoToFood, GatherFood, GoToHut, Rest,
  }

  private Tree target;
  private GathererAgentConfig config;
  private float nextActionTime;
  private int carrying;

  private GathererState state;
  private Dictionary<GathererState, AgentUpdate> updates = new Dictionary<GathererState, AgentUpdate>();
  private ForestController forest;
  private VillageController village;
  private AgentPather pather;

  public GathererAgent(GathererAgentConfig config){
    this.config = config;
    this.pather = new AgentPather(){transform=config.transform, speed=config.speed,arrivalDistance=config.arrivalDistance};
    forest = GameObject.FindObjectOfType<ForestController>();
    village = GameObject.FindObjectOfType<VillageController>();
    updates[GathererState.Idle] = UpdateIdle;
    updates[GathererState.GoToFood] = UpdateGoToFood;
    updates[GathererState.GatherFood] = UpdateGatherFood;
    updates[GathererState.GoToHut] = UpdateGoToHut;
    updates[GathererState.Rest] = UpdateRest;
    ReturnToRest();
  }

  public void Update(){
    updates[state]();
  }

  private void UpdateIdle(){
    target = forest.GetNearestFruitTreeTarget(config.transform.position);
    if(target != null && !village.IsStorageFull()){
      state = GathererState.GoToFood;
    }
  }
  private void UpdateGoToFood(){
    if(pather.ToPoint(target.transform.position)){
      ReturnToGather();
    }
  }

  private void UpdateGatherFood(){
    if(Time.time > nextActionTime){
      state = GathererState.GoToHut;
      carrying = forest.DefruitTree(target);
      target = null;
      return;
    }
  }

  private void UpdateGoToHut(){
    if(pather.ToPoint(village.GetNearestHut(config.transform.position).transform.position)){
      village.StoreFood(carrying);
      carrying = 0;
      ReturnToRest();
      return;
    }
  }

  public void ReturnToGather(){
    nextActionTime = Time.time + config.restRange.GetRangeValue();
    state = GathererState.GatherFood;
  }
  public void UpdateRest(){
    if(Time.time > nextActionTime){
      state = GathererState.Idle;
      return;
    }
  }

  private void ReturnToRest(){
    nextActionTime = Time.time + config.restRange.GetRangeValue();
    state = GathererState.Rest;
  }

  public bool IsBusy(){
    return !(state == GathererState.Idle || state == GathererState.Rest);
  }

  public void Release(){
    if(target != null) {
      forest.ReleaseTarget(target);
      target = null;
    }
  }

  public void Resume(){
    carrying = 0;
    ReturnToRest();
  }

  public int GetAvailability() {
      return statePriority[state];
  }
}