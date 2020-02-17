using Assets.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrushIntensity : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    float panelExpansionRate = 600.0f;
    bool expanded = false;
    bool sizeComplete = true;
    List<int> visibleChildren;
    public Painter painter;
    public GameObject brushIntensity;


    void Start() {

    }

    public void OnPointerEnter(PointerEventData eventData) {
        expanded = true;
        sizeComplete = false;
    }

    public void OnPointerExit(PointerEventData eventData) {
        expanded = false;
        sizeComplete = false;
    }

    public void OnMouseDown() {
        Debug.Log("Clicked");
    }

    public void OnClickChild(string child) {

    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KeyCode.LeftAlt))
            brushIntensity.SetActive(true);
        else
            brushIntensity.SetActive(false);
    }

    void resize(Vector2 amount, Vector2 min, Vector2 max) {
        RectTransform rectTransform = transform as RectTransform;

        Vector2 newSize = new Vector2(
            Mathf.Clamp(rectTransform.sizeDelta.x + amount.x, min.x, max.x),
            Mathf.Clamp(rectTransform.sizeDelta.y + amount.y, min.y, max.y));

        rectTransform.sizeDelta = newSize;
    }
}
