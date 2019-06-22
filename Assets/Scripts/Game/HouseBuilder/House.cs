using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    //Stores all tiles used by each floor of the house
    public Dictionary<int, HouseFloor> HouseFloors = new Dictionary<int, HouseFloor>();

    //Grid origin and parent object for the house
    public GameObject GridOrigin;
}
