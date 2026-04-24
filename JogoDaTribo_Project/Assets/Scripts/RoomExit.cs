using UnityEngine;

public class RoomExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        MapManager.Instance.AddClearedNode(MapManager.Instance.GetCurrentMapNodeID());
        MapManager.Instance.GoToScene("WorldMap");
    }
}
