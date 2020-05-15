using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class GamePrefabs {
    public GameObject ui;
}

public class GameController : MonoBehaviour {
    
    public GamePrefabs prefabs;
    

    void Start() {
        var ui = GameObject.Instantiate(prefabs.ui).GetComponent<UiController>();
        var villageController = GameObject.FindObjectOfType<VillageController>();
        var foodController = GameObject.FindObjectOfType<FoodController>();
        var forestController = GameObject.FindObjectOfType<ForestController>();
        var constructionController = GameObject.FindObjectOfType<ConstructionController>();
        var predatorController = GameObject.FindObjectOfType<PredatorController>();
        var babiesController = GameObject.FindObjectOfType<BabiesController>();
        
        ui.Init(villageController, foodController, babiesController, constructionController);
        foodController.Init(villageController);
        forestController.SpawnForest(villageController);
        constructionController.Init(villageController, forestController);
        predatorController.Init(villageController);
        babiesController.Init(villageController);
        villageController.SpawnVillage();

    }

}