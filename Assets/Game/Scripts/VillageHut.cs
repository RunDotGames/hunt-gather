using UnityEngine;

public class VillageHut : MonoBehaviour {

  public GameObject storageSprite;
  public GameObject notStorageSprite;

  private bool isStorage;
  
  public void SetIsStorage(bool isStorage){
    this.isStorage = isStorage;
    storageSprite.SetActive(isStorage);
    notStorageSprite.SetActive(!isStorage);
  }


}