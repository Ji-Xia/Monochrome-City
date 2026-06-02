using UnityEngine;

public class PlayerTeleportState : MonoBehaviour
{
    public float teleportCooldown = 0.2f;
    private float lastTeleportTime = -10f;

    public bool CanTeleport()
    {
        Debug.Log("CanTeleport: " + (Time.time - lastTeleportTime) + " seconds since last teleport");
        return Time.time - lastTeleportTime > teleportCooldown;

    }

    public void RecordTeleport()
    {
        lastTeleportTime = Time.time;
    }
}
