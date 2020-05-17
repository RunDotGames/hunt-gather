using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class FoodConfig {
  public int perHutCapacity;
  public int startingStorage;
  public int perEatAmount;
  public int perHarvestAmount;
  public RandomFloatRange eatTimeRange;
}

public class FoodController : MonoBehaviour {
  public FoodConfig config;
  
  public Action<int, int> OnFoodCapacityChange;
  public Action<int, int> OnFoodStorageChange;

  private int foodCapacity;
  private int foodStorage = -1;
  private VillageController village;

  public void Init(VillageController village){
    this.village = village;
    village.OnVillagerSpawned += HandleNewVillager;
    village.OnHutAllocationChange += HandleHutAllocationChange;
  }

  private void HandleHutAllocationChange(Dictionary<HutType, int> before, Dictionary<HutType, int> after){
    var oldCapacity = foodCapacity;
    foodCapacity = (after != null && after.ContainsKey(HutType.Storage) ? after[HutType.Storage]: 0) * config.perHutCapacity;
    Debug.Log("food capacity is now: " + foodCapacity + ", was: " + oldCapacity);
    if(oldCapacity == foodCapacity){
      return;
    }
    OnFoodCapacityChange?.Invoke(oldCapacity, foodCapacity);
    if(foodStorage < 0 && foodCapacity > 0){
      StoreFood(config.startingStorage);
    }
  }

  private void HandleNewVillager(Villager villager){
    villager.ProvideFeeder(FeedVillager, IsStorageFull, StoreHarvest, config.eatTimeRange, GetStoreLocation);
  }

  private Vector2 GetStoreLocation(Vector2 from){
    return village.GetNearestHut(from).transform.position;
  }


  public void StoreHarvest(){
    StoreFood(config.perHarvestAmount);
  }

  private void StoreFood(int amount){
    var priorFood = foodStorage;
    foodStorage = Math.Min(amount + foodStorage, foodCapacity);
    if(priorFood != foodStorage){
      OnFoodStorageChange?.Invoke(priorFood, foodStorage);
    }
  }

  public bool FeedVillager(){
    if(foodStorage < 0){
      return true;
    }
    if(foodStorage < config.perEatAmount){
      return false;
    }
    var oldStorage = foodStorage;
    foodStorage = foodStorage - config.perEatAmount;
    OnFoodStorageChange(oldStorage, foodStorage);
    return true;
  }

  public bool IsStorageFull(){
    return foodCapacity <= foodStorage;
  }

  public int GetCapacity(){
    return foodCapacity;
  }

  public int GetStorage(){
    return foodStorage;
  }
}