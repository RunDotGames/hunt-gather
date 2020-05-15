
using UnityEngine;
using System.Collections.Generic;
using System;

public interface HarvestTarget{
  Vector2 GetPostion();
  void Release();
  void Harvest();
  float GetHarvestTime();
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

  private HarvestTarget target;
  private AgentConfigCommon config;
  private float nextActionTime;
  private GathererState state;
  private Dictionary<GathererState, AgentUpdate> updates = new Dictionary<GathererState, AgentUpdate>();

  private AgentPather pather;
  private Func<bool> storageCheck;
  private Action store;
  private Func<Vector2, Vector2> storeLocation;
  private Func<Vector2, HarvestTarget> getTarget;
  
  public GathererAgent(AgentConfigCommon config){
    this.config = config;
    this.pather = new AgentPather(){transform=config.transform, speed=config.speed,arrivalDistance=config.arrivalDistance};
    updates[GathererState.Idle] = UpdateIdle;
    updates[GathererState.GoToFood] = UpdateGoToFood;
    updates[GathererState.GatherFood] = UpdateGatherFood;
    updates[GathererState.GoToHut] = UpdateGoToHut;
    updates[GathererState.Rest] = UpdateRest;
    ReturnToRest();
  }

  public void ProvideFeeder(Func<bool> storageCheck, Action store, Func<Vector2, Vector2> storeLocation){
    this.storageCheck = storageCheck;
    this.store = store;
    this.storeLocation = storeLocation;
  }

  public void ProvideHarvester(Func<Vector2, HarvestTarget> getTarget){
    this.getTarget = getTarget;
  }
  
  public void Update(){
    updates[state]();
  }

  private void UpdateIdle(){
    target = getTarget?.Invoke(config.transform.position) ?? null;
    if(target != null && !(storageCheck?.Invoke() ?? true)){
      state = GathererState.GoToFood;
    }
  }
  private void UpdateGoToFood(){
    if(pather.ToPoint(target.GetPostion())){
      ReturnToGather();
    }
  }

  private void UpdateGatherFood(){
    if(Time.time > nextActionTime){
      state = GathererState.GoToHut;
      target.Harvest();
      target = null;
      return;
    }
  }

  private void UpdateGoToHut(){
    if(storeLocation == null){
      return;
    }
    if(pather.ToPoint(storeLocation(config.transform.position))){
      store?.Invoke();
      ReturnToRest();
      return;
    }
  }

  public void ReturnToGather(){
    nextActionTime = Time.time + target.GetHarvestTime();
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
      target.Release();
      target = null;
    }
  }

  public void Resume(){
    ReturnToRest();
  }

  public int GetAvailability() {
      return statePriority[state];
  }
}