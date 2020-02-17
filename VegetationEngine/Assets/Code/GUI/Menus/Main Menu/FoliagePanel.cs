using Assets;
using Assets.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoliagePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    float panelExpansionRate = 600.0f;
    bool expanded = false;
    bool sizeComplete = true;
    List<int> visibleChildren;
    public Painter painter;
    GameObject[] buttons = new GameObject[3];
    public GameObject testObject;
    public Material transparent;
    GridManager gridManager;

    void Start() {
        buttons[0] = transform.Find("FoliageLow").gameObject;
        buttons[1] = transform.Find("FoliageMed").gameObject;
        buttons[2] = transform.Find("FoliageHigh").gameObject;

        Toggle btnLow = buttons[0].GetComponent<Toggle>();
        Toggle btnMed = buttons[1].GetComponent<Toggle>();
        Toggle btnHigh = buttons[2].GetComponent<Toggle>();

        btnLow.onValueChanged.AddListener(delegate { OnClickChild(0); });
        btnMed.onValueChanged.AddListener(delegate { OnClickChild(1); });
        btnHigh.onValueChanged.AddListener(delegate { OnClickChild(2); });

        visibleChildren = new List<int>();

		//foreach (GridElement element in mainGridElements)
		//    element.TestElement = 5;

		//int count = AddElement(parentGrid, new Vector3(0.0f, 0.0f, 0.0f));
		//Debug.Log(count);
    }

    int AddElement(GridElement parent, Vector3 displacement) {
        int count = parent.Elements.Count;
        foreach (GridElement element in parent.Elements) {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = element.Position + displacement;
            cube.transform.localScale = element.Size;
            cube.GetComponent<Renderer>().material = transparent;

            if (element.Elements.Count > 0)
                count += AddElement(element, new Vector3(displacement.x, displacement.y + 1050.0f, displacement.z));
        }

        return count;
    }

    void SetChildState(int child, bool state) {
        if (child >= 0 && child < 3) {
            Toggle toggle = buttons[child].GetComponent<Toggle>();
            ColorBlock colors = toggle.colors;

            painter.setPainterType(child, state);

            if (state) {
                colors.normalColor = UIDesign.Buttons[(int)UIDesign.Elements.Selected];
                colors.highlightedColor = UIDesign.Buttons[(int)UIDesign.Elements.Selected];
            }
            else {
                colors.normalColor = UIDesign.Buttons[(int)UIDesign.Elements.Default];
                colors.highlightedColor = UIDesign.Buttons[(int)UIDesign.Elements.Highlighted];
            }

            toggle.colors = colors;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void OnClickChild(int child) {
        if (child >= 0 && child < 3) {
            Toggle toggle = buttons[child].GetComponent<Toggle>();
            SetChildState(child, toggle.isOn);
        }
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
        Transform child;
        Color color;

        if (Input.GetMouseButtonDown(1))
            for (int i = 0; i < 3; i++) {
                painter.setPainterType(i, false);
                SetChildState(i, false);
            }

        if (Input.GetKeyDown(KeyCode.T))
            for (int i = 0; i < 3; i++) {
                painter.setPainterType(i, true);
                SetChildState(i, true);
            }

        if (!sizeComplete) {
            int direction = (expanded) ? 1 : -1;

            resize(new Vector2(panelExpansionRate * Time.deltaTime * direction, 0.0f), new Vector2(50.0f, 50.0f), new Vector2(200.0f, 50.0f));

            RectTransform childRect;
            RectTransform rectTransform = transform as RectTransform;

            if (expanded) {
                for (int i = 0; i < transform.childCount; i++) {
                    child = transform.GetChild(i);

                    childRect = transform.GetChild(i) as RectTransform;

                    if (childRect.localPosition.x + childRect.sizeDelta.x < rectTransform.sizeDelta.x && childRect.localPosition.y + childRect.sizeDelta.y < rectTransform.sizeDelta.y) {
                        visibleChildren.Add(i);
                        child.gameObject.SetActive(true);
                    }
                }
            }
            else {
                for (int i = 0; i < transform.childCount; i++) {
                    child = transform.GetChild(i);

                    childRect = transform.GetChild(i) as RectTransform;

                    if (childRect.localPosition.x + childRect.sizeDelta.x > rectTransform.sizeDelta.x || childRect.localPosition.y + childRect.sizeDelta.y > rectTransform.sizeDelta.y) {
                        if (visibleChildren.Contains(i))
                            visibleChildren.Remove(i);

                        color = child.GetComponent<Image>().color;
                        color.a = 0.0f;
                        child.GetComponent<Image>().color = color;
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        for (int i = visibleChildren.Count - 1; i > -1; i--) {
            child = transform.GetChild(visibleChildren[i]);

            color = child.GetComponent<Image>().color;
            color.a += Time.deltaTime;

            if (color.a > 1.0f) {
                color.a = 1.0f;
                visibleChildren.RemoveAt(i);
            }

            child.GetComponent<Image>().color = color;
        }
	}

    void resize(Vector2 amount, Vector2 min, Vector2 max) {
        RectTransform rectTransform = transform as RectTransform;

        Vector2 newSize = new Vector2(
            Mathf.Clamp(rectTransform.sizeDelta.x + amount.x, min.x, max.x),
            Mathf.Clamp(rectTransform.sizeDelta.y + amount.y, min.y, max.y));

        rectTransform.sizeDelta = newSize;
    }
}
