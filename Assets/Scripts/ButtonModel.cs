using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonModel : MonoBehaviour
{
    public GameObject checkbox;
    public GameObject SaveBtn;
    public Button[,] checkboxes;
    GameObject panel;
    bool panelActive;

    // Start is called before the first frame update
    void Start()
    {
        GUI gui = FindObjectOfType<GUI>();
        checkboxes = new Button[gui.planeRow, gui.planeCol];
        GenCheckboxes();
        SetButtonCallbacks();
        panelActive = false;
        panel = GameObject.Find("Panel");
        panel.SetActive(panelActive);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void GenCheckboxes()
    {
        GUI gui = FindObjectOfType<GUI>();
        GameObject ObstacleSelection = GameObject.Find("ObstacleSelection");

        GameObject btn = GameObject.Instantiate(SaveBtn, ObstacleSelection.transform);
        btn.name = "Save Btn";
        btn.GetComponentInChildren<TextMeshProUGUI>().text = "Save";
        btn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16;
        btn.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        btn.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        RectTransform rtSavebtn = btn.GetComponent<RectTransform>();
        rtSavebtn.sizeDelta = new Vector2(80f, 30f);
        Vector2 size = rtSavebtn.sizeDelta;
        rtSavebtn.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 5f, size.x);
        rtSavebtn.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 5f, size.y);

        for (int i = 0; i < gui.planeRow; i ++)
        for (int j = 0; j < gui.planeCol; j ++)
        {
            btn = GameObject.Instantiate(checkbox, ObstacleSelection.transform);
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = Vector2.one * 15f;
            size = rt.sizeDelta;
            float x = (gui.planeCol - j) * (size.x + 2f);
            float y = (gui.planeRow - i) * (size.y + 2f);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, x, size.x);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, y + rtSavebtn.sizeDelta.y + 10f, size.y);
            btn.GetComponent<Checkbox>().Setup(i, j);
            checkboxes[i, j] = btn.GetComponent<Button>();
        }
    }

    void SetButtonCallbacks()
    {
        GUI gui = FindObjectOfType<GUI>();
        Button GUImenu = GameObject.Find("GUI menu").GetComponent<Button>();
        GUImenu.onClick.AddListener(GUImenuOnClick);
        Button _SaveBtn = GameObject.Find("Save Btn").GetComponent<Button>();
        _SaveBtn.onClick.AddListener(SaveBtnOnClick);
    }

    void GUImenuOnClick()
    {
        panelActive = !panelActive;
        panel.SetActive(panelActive);
    }

    void SaveBtnOnClick()
    {
        GUI gui = FindObjectOfType<GUI>();
        gui.Reset();
        ObstacleModel obm = FindObjectOfType<ObstacleModel>();
        obm.SetObstaclesFromGUI();
        gui.Setup();
    }
}
