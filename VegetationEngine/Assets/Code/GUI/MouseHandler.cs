using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseHandler : MonoBehaviour {
    public Material matUICurrent;
    public Material matDefault;
    public Material matTinted;
    Color uiPanelColor;
    Color defaultColor = new Color(0.0f, 0.0f, 0.0f, 14.0f / 255.0f);
    Color tintedColor = new Color(56.0f / 255.0f, 106.0f / 255.0f, 141.0f / 255.0f, 255.0f / 255.0f);
    public float uiTintSpeed;
    float currentUITint;
    public GameObject foligePanel;
    public bool openFoliagePanel;
    float panelExpansionRate = 600.0f;
    

    // Use this for initialization
    void Start () {
        defaultColor = matDefault.color;
        tintedColor = matTinted.color;

        uiPanelColor = defaultColor;
        matUICurrent.color = uiPanelColor;
    }

    void changePanelSize(GameObject panel, Vector2 amount, Vector2 min, Vector2 max) {
        RectTransform rectTransform = panel.transform as RectTransform;

        Vector2 newSize = new Vector2(
            Mathf.Clamp(rectTransform.sizeDelta.x + amount.x, min.x, max.x),
            Mathf.Clamp(rectTransform.sizeDelta.y + amount.y, min.y, max.y));
        
        rectTransform.sizeDelta = newSize;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            if (EventSystem.current.IsPointerOverGameObject()) {
                //uiPanel.material = matTinted;
                //Debug.Log("Clicked on the UI");
                //PanelState state = new PanelState(foligePanel, true, new Vector2(150.0f, 50.0f));
            }
        }

        if (EventSystem.current.IsPointerOverGameObject()) {
            currentUITint = Mathf.Clamp(currentUITint + Time.deltaTime * uiTintSpeed, 0.0f, 1.0f);
            matUICurrent.color = Maths.mix(matDefault.color, matTinted.color, currentUITint);
            //Debug.Log("OVER PANEL");

            //changePanelSize(foligePanel, new Vector2(panelExpansionRate * Time.deltaTime, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(150.0f, 50.0f));
        }
        else {
            currentUITint = Mathf.Clamp(currentUITint - Time.deltaTime * uiTintSpeed, 0.0f, 1.0f);
            matUICurrent.color = Maths.mix(matDefault.color, matTinted.color, currentUITint);
            //changePanelSize(foligePanel, new Vector2(panelExpansionRate * -Time.deltaTime, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(150.0f, 50.0f));
            //Debug.Log("(" + foligePanel.rectTransform.rect.width + ", " + foligePanel.rectTransform.rect.height + ")");
        }
    }
}
