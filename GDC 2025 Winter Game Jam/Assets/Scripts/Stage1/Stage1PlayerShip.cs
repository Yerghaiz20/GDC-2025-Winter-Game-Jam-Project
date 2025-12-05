using UnityEngine;
using System;

public class DefendedShip : MonoBehaviour
{
    // Scene Manager assigns a handler to this
    public Action<DefendedShip> OnShipDestroyed;

    bool isDestroyed = false;

    // Call this when the ship gets hit or otherwise destroyed
    public void DestroyShip()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        OnShipDestroyed?.Invoke(this);
        Destroy(gameObject);
    }
}

