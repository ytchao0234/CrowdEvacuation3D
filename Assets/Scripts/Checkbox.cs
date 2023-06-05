using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkbox : MonoBehaviour
{
    public int i, j;
    public int select = 0; // Empty, Movable, Immovable
    int selectNumber = 4;
    public string type = ""; // Valid, Bound, Exit
    Button checkbtn;

    // Start is called before the first frame update
    void Start()
    {
        checkbtn = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(int i, int j)
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        this.i = i;
        this.j = j;
        this.select = 0;
        if (!(this.i == 0 || this.i == gui.planeRow - 1 || this.j == 0 || this.j == gui.planeCol - 1))
            this.type = "Valid";
        SetButtonColor(Color.white);
        SetOnclickCallback();
        SetExitOnclickCallback();
        
        if (this.i == 0 || this.i == gui.planeRow - 1 || this.j == 0 || this.j == gui.planeCol - 1)
        {
            type = "Bound";
            SetButtonColor(Color.gray);
            fm.floor[i, j].transform.tag = "ImmovableObstacle";
        }

    }

    void SetButtonColor(Color color)
    {
        var colors = GetComponent<Button>().colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.1f;
        colors.pressedColor = color * 0.5f;
        colors.selectedColor = color;
        colors.disabledColor = color * 0.9f;
        GetComponent<Button>().colors = colors;
    }

    public void SetOnclickCallback()
    {
        GetComponent<Button>().onClick.AddListener(CheckboxOnClick);
    }

    void CheckboxOnClick()
    {
        // Debug.Log(i.ToString() + ", " + j.ToString());
        if (type != "Valid") return;

        select = (select + 1) % selectNumber;

        switch (select)
        {
            case 0:
                SetButtonColor(Color.white);
                break;
            case 1:
                SetButtonColor(Color.cyan * 0.8f);
                break;
            case 2:
                SetButtonColor(Color.gray);
                break;
            case 3:
                SetButtonColor(Color.red);
                FindObjectOfType<GUI>().disaster_cell = new Vector2Int(i,j);
                break;
            default:
                break;
        }

    }

    public void SetExitOnclickCallback()
    {
        GetComponent<Button>().onClick.AddListener(ExitCheckboxOnClick);
    }

     void ExitCheckboxOnClick()
    {
        GUI gui = FindObjectOfType<GUI>();
        FloorModel fm = FindObjectOfType<FloorModel>();
        // Debug.Log(i.ToString() + ", " + j.ToString());
        Vector2Int temp = new Vector2Int(i,j);
        if (type == "Valid") return;

        if (fm.floor[i, j].transform.tag == "Exit")
        {
            type = "Bound";
            SetButtonColor(Color.gray);
            fm.floor[i, j].transform.tag = "ImmovableObstacle";
        }
        else
        {
            type = "Exit";
            SetButtonColor(Color.green * 0.8f);
            fm.floor[i, j].transform.tag = "Exit";
        }

    }
}
