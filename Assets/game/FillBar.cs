using UnityEngine;


public class FillBar : MonoBehaviour{
  public RectTransform fill;
  private int capacity;
  private int value;

  public void SetCapcity(int capacity){
    this.capacity = capacity;
    UpdateView();
  }
  public void SetValue(int value){
    this.value = value;
    UpdateView();
  }

  private void UpdateView(){
    float percent = Mathf.Min(value, capacity) / Mathf.Max(capacity, 1.0f);
    fill.anchorMax = new Vector2(fill.anchorMax.x, percent);
  }

}