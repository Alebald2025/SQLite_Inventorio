using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ItemsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI swordNum;
    [SerializeField] TextMeshProUGUI meatNum;
    [SerializeField] TextMeshProUGUI enderNum;
    [SerializeField] TextMeshProUGUI cespedNum;

    public int SwordCantidad;
    public int MeatCantidad;
    public int EnderCantidad;
    public int CespedCantidad;

    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        swordNum.text = SwordCantidad.ToString();
        meatNum.text = MeatCantidad.ToString();
        enderNum.text = EnderCantidad.ToString();
        cespedNum.text = CespedCantidad.ToString();
    }

    public void AddSword()
    {
        SwordCantidad++;
    }

    public void DeleteSword()
    {
        SwordCantidad--;
    }

    public void AddMeat()
    {
        MeatCantidad++;
    }

    public void DeleteMeat()
    {
        MeatCantidad--;
    }

    public void AddEnder()
    {
        EnderCantidad++;
    }

    public void DeleteEnder()
    {
        EnderCantidad--;
    }

    public void AddCesped()
    {
        CespedCantidad++;
    }

    public void DeleteCesped()
    {
        CespedCantidad--;
    }
}
