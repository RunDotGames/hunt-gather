using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class VillagerConfig {
  public GameObject prefab;
  public float speed;
  public float arrivalDistance;
  public float attackDistance;
  public float gatherTime;
  public RandomFloatRange restRange;
  public RandomFloatRange hungerRange;
  public RandomIntRange workRange;
  public float attackPace;
  public float prowlPace;
  public float spotDistance;
  public Color gatherColor;
  public Color hunterColor;
  public Color builderColor;
}

[Serializable]
public class VillageConfig {
  public VillagerAllocation[] startingAllocations;
  public int startingStorage;
  public GameObject prefab;
  public float hutRadius;
  public int hutCapacity;
  public int feedAmount;
}

[Serializable]
public class VillagerAllocation {
  public VillagerType type;
  public int count;
}

public enum VillagerType {
  Gatherer, Hunter, Builder
}


public delegate void OnValueChange<T>(T oldValue, T newValue);

public class VillageController: MonoBehaviour {
  public static readonly int villagerTypecount = Enum.GetNames(typeof(VillagerType)).Length;
  public Transform villageCenter;
  public VillageConfig villageConfig;
  public VillagerConfig villagerConfig;
  private VillageHut primaryHut;
  private List<VillageHut> huts = new List<VillageHut>();
  private List<Villager> villagers = new List<Villager>();
  private Dictionary<VillagerType, List<Villager>> villagersByType;

  public OnValueChange<int> OnFoodCapacityChange;
  public OnValueChange<int> OnFoodStorageChange;
  public OnValueChange<Dictionary<VillagerType, int>> OnAllocationChange;

  private int foodCapacity;
  private int foodStorage;
  
  public void SpawnVillage(){
    var predatorController = GameObject.FindObjectOfType<PredatorController>();
    primaryHut = SpawnHut(villageCenter.transform.position);
    villagersByType = new Dictionary<VillagerType, List<Villager>>();
    villageConfig.startingAllocations.Select((item)=>{
      var type = item.type;
      villagersByType[type] = new List<Villager>();
      for(var i = 0; i < item.count; i++){
        SpawnVillager(type, predatorController.GetNearestPredator, primaryHut);
      }
      return item.count;
    }).ToArray();
    Debug.Log("village init");
    StoreFood(villageConfig.startingStorage);
  }

  public Dictionary<VillagerType, int> GetCurrentAllocation(){
    return villagersByType?.Keys.Aggregate(new Dictionary<VillagerType, int>(), (result, item) => {
      result[item] = villagersByType[item].Count;
      return result;
    }) ?? new Dictionary<VillagerType, int>();
  }

  public VillageHut GetNearestHut(Vector2 from){
    return DistanceUtility.GetNearest(from, huts);
  }
  public VillageHut GetPrimaryHut(){
    return primaryHut;
  }

  public VillageHut SpawnHut(Vector2 position){
    var hut = GameObject.Instantiate(villageConfig.prefab).GetComponent<VillageHut>();
    hut.transform.position = position;
    var oldCapacity = foodCapacity;
    foodCapacity = foodCapacity + villageConfig.hutCapacity;
    huts.Add(hut);
    OnFoodCapacityChange?.Invoke(oldCapacity, foodCapacity);
    return hut;
  }

  public Tuple<int, int> GetFoodCapactiyAndStorage(){
    return new Tuple<int, int>(foodCapacity, foodStorage);
  }

  private void SpawnVillager(VillagerType type, Func<Vector2, CombatTarget> targetProvider, VillageHut hut){
    var priorAllocation = GetCurrentAllocation();
    var villager = GameObject.Instantiate(villagerConfig.prefab).GetComponent<Villager>();
    villager.Init(villagerConfig, FeedVillager, type, targetProvider);
    villagers.Add(villager);
    villager.transform.position = (UnityEngine.Random.insideUnitCircle.normalized * villageConfig.hutRadius) + (Vector2)hut.transform.position;
    villager.OnDeath += HandleVillagerDeath;
    villagersByType[type].Add(villager);
    OnAllocationChange?.Invoke(priorAllocation, GetCurrentAllocation());
  }

  private void HandleVillagerDeath(CombatTarget target){
    var villager = (Villager)target;
    var priorAllocation = GetCurrentAllocation();
    villager.OnDeath -= HandleVillagerDeath;
    villagers.Remove(villager);
    var type = villager.GetVillagerType();
    villagersByType[type].Remove(villager);
    OnAllocationChange?.Invoke(priorAllocation, GetCurrentAllocation());
  }

  public void StoreFood(int quantity){
    var priorFood = foodStorage;
    foodStorage = Math.Min(quantity + foodStorage, foodCapacity);
    if(priorFood != foodStorage){
      OnFoodStorageChange?.Invoke(priorFood, foodStorage);
    }
  }

  private bool FeedVillager(){
    if(foodStorage < villageConfig.feedAmount){
      return false;
    }
    var oldStorage = foodStorage;
    foodStorage = foodStorage - villageConfig.feedAmount;
    OnFoodStorageChange(oldStorage, foodStorage);
    return true;
  }

  public bool IsStorageFull(){
    return foodCapacity <= foodStorage;
  }

  public Villager getNearestVillager(Vector2 from){
    return DistanceUtility.GetNearest(from, villagers);
  }


  public void ReAllocate(Dictionary<VillagerType, int> newAllocation){
    var oldAllocation = GetCurrentAllocation();
    var released = newAllocation.Keys.Aggregate(new List<Villager>(), (result, item) =>{
      var dif = oldAllocation[item] - newAllocation[item];
      if(dif <= 0){
        return result;
      }
      villagersByType[item].Sort((first, second) => first.GetAvailability() - second.GetAvailability());
      for(int i = 0; i < dif; i++){
        var villager = villagersByType[item][0];
        villagersByType[item].RemoveAt(0);
        result.Add(villager);
      }
      return result;
    });

    var unaccounted = newAllocation.Keys.Aggregate(released, (result, item) =>{
      var dif = newAllocation[item] - oldAllocation[item];
      if(dif <= 0){
        return result;
      }
      if(result.Count == 0) {
        Debug.LogError("reallocation error state, no released villagers for re-assignment");
        return result;
      }
      for(int i = 0; i < dif; i++){
        var villager = result[0];
        villagersByType[item].Add(villager);
        villager.SetVillagerType(item);
        result.RemoveAt(0);
      }
      return result;
    });
    if(unaccounted.Count > 0){
      Debug.LogError("reallocation error state, released villagers not reassigned");
    }
  }
}