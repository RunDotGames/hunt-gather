using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class VillagerConfig {
  public GameObject prefab;
  public float speed;
  public float arrivalDistance;
  public RandomFloatRange restRange;
  
  public Color gatherColor;
  public Color hunterColor;
  public Color builderColor;
}

[Serializable]
public class VillageConfig {
  public VillagerAllocation[] startingAllocations;
  public GameObject prefab;
  public float hutRadius;
}

[Serializable]
public class VillagerAllocation {
  public VillagerType type;
  public int count;
}

public enum VillagerType {
  Gatherer, Hunter, Builder
}

public enum HutType {
  Storage, Housing
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
  private Dictionary<HutType, List<VillageHut>> hutsByType = new Dictionary<HutType, List<VillageHut>>();

  
  public event OnValueChange<Dictionary<VillagerType, int>> OnVillagerAllocationChange;
  public event OnValueChange<Dictionary<HutType, int>> OnHutAllocationChange;
  public event Action<Villager> OnVillagerSpawned;
  
  public void SpawnVillage(){
    var predatorController = GameObject.FindObjectOfType<PredatorController>();
    
    hutsByType[HutType.Housing] = new List<VillageHut>();
    hutsByType[HutType.Storage] = new List<VillageHut>();
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
  }

  public Dictionary<HutType, int> GetCurrentHutAllocation(){
    return GetAllocation(hutsByType);
  }
  public Dictionary<VillagerType, int> GetCurrentVillagerAllocation(){
    return GetAllocation(villagersByType);
  }

  public IEnumerable<Villager> GetVillagers(){
    return villagers;
  }

  private Dictionary<K, int> GetAllocation<K, V>(Dictionary<K, List<V>> map){
    return map?.Keys.Aggregate(new Dictionary<K, int>(), (result, item) => {
      result[item] = map[item].Count;
      return result;
    }) ?? new Dictionary<K, int>();
  }

  public VillageHut GetNearestHut(Vector2 from){
    return DistanceUtility.GetNearest(from, huts);
  }
  public VillageHut GetPrimaryHut(){
    return primaryHut;
  }

  public VillageHut SpawnHut(Vector2 position){
    var priorAllocation = GetCurrentHutAllocation();
    var hut = GameObject.Instantiate(villageConfig.prefab).GetComponent<VillageHut>();
    hut.transform.position = position;
    huts.Add(hut);
    hutsByType[HutType.Storage].Add(hut);
    OnHutAllocationChange?.Invoke(priorAllocation, GetCurrentHutAllocation());
    return hut;
  }

  private void SpawnVillager(VillagerType type, Func<Vector2, CombatTarget> targetProvider, VillageHut hut){
    var priorAllocation = GetCurrentVillagerAllocation();
    var villager = GameObject.Instantiate(villagerConfig.prefab).GetComponent<Villager>();
    villager.Init(villagerConfig, type, targetProvider);
    villagers.Add(villager);
    villager.transform.position = (UnityEngine.Random.insideUnitCircle.normalized * villageConfig.hutRadius) + (Vector2)hut.transform.position;
    villager.OnDeath += HandleVillagerDeath;
    villagersByType[type].Add(villager);
    OnVillagerAllocationChange?.Invoke(priorAllocation, GetCurrentVillagerAllocation());
    OnVillagerSpawned?.Invoke(villager);
  }

  private void HandleVillagerDeath(CombatTarget target){
    var villager = (Villager)target;
    var priorAllocation = GetCurrentVillagerAllocation();
    villager.OnDeath -= HandleVillagerDeath;
    villagers.Remove(villager);
    var type = villager.GetVillagerType();
    villagersByType[type].Remove(villager);
    OnVillagerAllocationChange?.Invoke(priorAllocation, GetCurrentVillagerAllocation());
  }

  public Villager getNearestVillager(Vector2 from){
    return DistanceUtility.GetNearest(from, villagers);
  }


  public void ReAllocateVillagers(Dictionary<VillagerType, int> newAllocation){
    var oldAllocation = GetCurrentVillagerAllocation();
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