using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class AllocSlider : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {

    [SerializeField] private GameObject[] handles;
    [SerializeField] private int count = 10;

    private RectTransform myTransform;
    private GameObject targetHandle;
    private GameObject leftHandle;
    private GameObject rightHandle;

    protected override void Start() {
        base.Start();
        myTransform = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        var targetRect = targetHandle.GetComponent<RectTransform>();
        var leftBound = (leftHandle?.GetComponent<RectTransform>().position.x ?? myTransform.position.x-targetRect.rect.width) + targetRect.rect.width;
        var rightBound = (rightHandle?.GetComponent<RectTransform>().position.x ?? myTransform.rect.width+targetRect.rect.width) - targetRect.rect.width;
        
        var xPos = Mathf.Max(leftBound, Mathf.Min(eventData.position.x, rightBound));
        targetRect.position = new Vector3(xPos, targetRect.position.y, targetRect.position.z);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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
        
        var targetX = targetHandle.GetComponent<RectTransform>().position.x;
        var boundHandles = handles.Aggregate( new GameObject[2], (result, item) => {
            if(item == targetHandle){
                return result;
            }
            
            var handleX = item.GetComponent<RectTransform>().position.x;
            if(handleX < targetX){
                if(result[0] == null){
                    result[0] = item;
                    return result;
                }
                if(result[0].GetComponent<RectTransform>().position.x < handleX){
                    result[0] = item;
                    return result;
                }
                return result;
            }
            if(result[1] == null){
                result[1] = item;
                return result;
            }
            if(result[1].GetComponent<RectTransform>().position.x > handleX){
                result[1] = item;
                return result;
            }
            return result;
        });
        leftHandle = boundHandles[0];
        rightHandle = boundHandles[1];
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var handleWidth =targetHandle.GetComponent<RectTransform>().rect.width;
        var totalWidth = myTransform.rect.width - handleWidth * 0.5f * handles.Length;
        var positions = handles.Select((handle) => {
            var transform = handle.GetComponent<RectTransform>();
            return transform.position.x;
        }).Concat(new float[] {myTransform.rect.width}).ToArray();
        var values =positions.Select((position, index)=>{
            var start = handleWidth;
            if(index == positions.Length-1){
                start = positions[index -1];
            } else if(index > 0) {
                start = positions[index - 1] + handleWidth;
            }
            var precent = (position - start) / totalWidth;
            Debug.Log(position + "-" + precent);
            return Mathf.FloorToInt(precent* (float)count);
        }).ToArray();
        var total = values.Aggregate(0, (agg, current) => agg + current);
        for(int i = 0; i < count - total; i++){
            values[i] = values[i] + 1;
        }
        Debug.Log(values.Aggregate("", (agg, current) => agg + ", " + current));

        
    }
}
