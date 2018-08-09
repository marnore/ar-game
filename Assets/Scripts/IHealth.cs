using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary> Interface for Objects with concept of Health </summary>*/
public interface IHealth
{

    /**<summary> Current hit points of the object </summary>*/
    int hp { get; set; }

    /**<summary> Callback function when health changes </summary>*/
    Action<int, int> OnHealthChanged { get; set; }
}
