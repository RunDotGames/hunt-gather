
using UnityEngine;
using System.Collections.Generic;
using System;

public interface HarvestTarget{
  Vector2 GetPostion();
  void Release();
  void Harvest();
  RandomFloatRange GetHarvestTime();
}

public class GathererAgent: Agent {

  public event Action<EntityEventType> onEvent;

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
  private SeasonalTimeRange harvestTime;
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
      harvestTime = new SeasonalTimeRange(target.GetHarvestTime());
      state = GathererState.GoToFood;
      onEvent?.Invoke(EntityEventType.Walk);
    }
  }
  private void UpdateGoToFood(){
    if(!pather.ToPoint(target.GetPostion())){
      return;
    }
    harvestTime.Resume();
    state = GathererState.GatherFood;
    onEvent?.Invoke(EntityEventType.Gather);
  }

  private void UpdateGatherFood(){
    if(!harvestTime.Update(SeasonTask.Harvest)){
      return;
    }
    state = GathererState.GoToHut;
    onEvent?.Invoke(EntityEventType.Walk);
    harvestTime.Stop();
    harvestTime = null;
    target.Harvest();
    target = null;
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

  public void UpdateRest(){
    if(!config.restRange.Update(SeasonTask.Rest)){
      return;
    }
    config.restRange.Stop();
    state = GathererState.Idle;
    onEvent?.Invoke(EntityEventType.Idle);
  }

  private void ReturnToRest(){
    config.restRange.Resume();
    state = GathererState.Rest;
    onEvent?.Invoke(EntityEventType.Rest);
  }

  public bool IsBusy(){
    return !(state == GathererState.Idle || state == GathererState.Rest);
  }

  public void Release(){
    if(target != null) {
      target.Release();
      target = null;
      harvestTime.Stop();
      harvestTime = null;
    }
  }

  public void Resume(){
    ReturnToRest();
  }

  public int GetAvailability() {
      return statePriority[state];
  }
}