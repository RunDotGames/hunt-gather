using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class GamePrefabs {
    public GameObject ui;
}

public class GameController : MonoBehaviour {
    
    public GamePrefabs prefabs;
    
    private bool checkOnUpdate = false;
    private VillageController villageController;
    void Start() {
        ScoreController.Reset();
        var ui = GameObject.Instantiate(prefabs.ui).GetComponent<UiController>();
        villageController = GameObject.FindObjectOfType<VillageController>();
        var foodController = GameObject.FindObjectOfType<FoodController>();
        var forestController = GameObject.FindObjectOfType<ForestController>();
        var constructionController = GameObject.FindObjectOfType<ConstructionController>();
        var predatorController = GameObject.FindObjectOfType<PredatorController>();
        var babiesController = GameObject.FindObjectOfType<BabiesController>();
        var seasonController = GameObject.FindObjectOfType<SeasonController>();

        villageController.OnVillagerSpawned += HandleVillagerSpawned;

        seasonController.Init();
        ui.Init(villageController, foodController, babiesController, constructionController);
        foodController.Init(villageController);
        forestController.SpawnForest(villageController);
        constructionController.Init(villageController, forestController);
        predatorController.Init(villageController);
        babiesController.Init(villageController);
        villageController.SpawnVillage();


    }

    private void HandleVillagerSpawned(Villager villager){
        Debug.Log("spawned");
        checkOnUpdate = true;
    }


    public void Update(){
        if(!checkOnUpdate){
            return;
        }
        
        if(villageController.GetVillagerCount() == 0){
            Debug.Log("end loading");
            checkOnUpdate = false;
            GameObject.Destroy(gameObject);
            SceneManager.LoadScene("end-demo");
        }
    }

}