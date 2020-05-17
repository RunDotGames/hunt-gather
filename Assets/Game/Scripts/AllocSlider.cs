using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System;

public class AllocSlider : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {

    [SerializeField] private int count = 10;
    [SerializeField] private GameObject[] handles;
    [SerializeField] private GameObject[] labels;
    [SerializeField] private int[] handlePositions;
    private GameObject targetHandle;
    private int targetHandleIndex;
    private int[] allocations;

    private bool isDragCancel;

    public event Action<int[], int> OnChange;

    public void Init() {
        base.Start();
        
        Array.Sort(handlePositions);
        allocations = new int[handlePositions.Length+1];
        UpdateForPositions();
    }

    private void UpdateForPositions(){
        var myTransform = (RectTransform)transform;
        var increment = 1.0f / count;
        
        var left = myTransform.position.x;
        var handleWidth = handles[0].GetComponent<RectTransform>().rect.width;
        handlePositions.Select((position, index) => {
            var prevPosition = index > 0 ? handlePositions[index-1] : -1;
            var nextPosition = index < handlePositions.Length-1 ? handlePositions[index+1] : count+1;
            var  transform = handles[index].GetComponent<RectTransform>();
            var xPos = 0.0f;
            if(prevPosition == position){
                xPos = handleWidth * 0.5f;
            }
            if(nextPosition == position) {
                xPos = -handleWidth * 0.5f;
            }
            transform.anchoredPosition = new Vector3(xPos, 0);
            transform.anchorMax = new Vector2(increment*position, 1.0f);
            transform.anchorMin = new Vector2(increment*position, 0);
            //transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
            return 0;
        }).ToArray();
        for(var i = 0; i < allocations.Length; i++){
            allocations[i] = (i == handlePositions.Length ? count : handlePositions[i]) - ((i == 0) ? 0 : handlePositions[i-1]);
            var end = i == handlePositions.Length ? myTransform.rect.width : handles[i].GetComponent<RectTransform>().position.x;
            var start = (i == 0) ? 0 : handles[i-1].GetComponent<RectTransform>().position.x;
            var labelX = start +  (end - start)/2;
            labels[i].transform.position = new Vector3(labelX, labels[i].transform.position.y, labels[i].transform.position.z);
            labels[i].GetComponent<Text>().text = allocations[i].ToString();
        }
    }

    public void OnDrag(PointerEventData eventData){
        var myTransform = (RectTransform)transform;
        if(isDragCancel){
            return;
        }
        var min = targetHandleIndex > 0 ? handlePositions[targetHandleIndex-1] : 0;
        var max = targetHandleIndex < handles.Length-1 ? handlePositions[targetHandleIndex+1] : count;
        var dragTarget = Mathf.RoundToInt( (eventData.position.x - myTransform.position.x) / ( (myTransform.rect.width * myTransform.lossyScale.x) / count));
        var boundedDrag = Math.Max(min, Math.Min(max, dragTarget));
        if(handlePositions[targetHandleIndex] != boundedDrag){
            handlePositions[targetHandleIndex] = boundedDrag;
            UpdateForPositions();
        }
    }

    public void SetState(int[] positions, int count){
        var myTransform = (RectTransform)transform;
        if(handlePositions.Length != positions.Length || count == 0){
            return;
        }
        isDragCancel = true;
        this.count = count;
        this.handlePositions = positions.Aggregate(new List<int>(), (result, item) => {
            result.Add(Math.Max(Math.Min(item, count), 0));
            return result;
        }).ToArray();
        if(myTransform == null){
            return;
        }
        UpdateForPositions();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        isDragCancel = false;
        targetHandle = handles.Aggregate(null as GameObject, (result, item) => {
            if(result == null){
                return item;
            }
            var aggDist = Mathf.Abs(result.GetComponent<RectTransform>().position.x - eventData.position.x);
            var currentDist = Mathf.Abs(item.GetComponent<RectTransform>().position.x - eventData.position.x);
            if(currentDist <= aggDist){
                return item;
            }
            return result;
        });
        targetHandleIndex = Array.IndexOf(handles, targetHandle);
    }

    public void OnEndDrag(PointerEventData eventData) {
        if(isDragCancel){
            return;
        }
        OnChange?.Invoke(handlePositions, count);

    }
}
