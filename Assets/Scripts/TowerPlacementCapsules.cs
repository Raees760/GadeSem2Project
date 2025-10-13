using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacementCapsules : MonoBehaviour
{
    private bool isOccupied = false;

    void OnMouseDown()
    {
        // Check if the mouse is currently over a UI element.
        // If it is, do not proceed with placing the tower.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }
        // Check if a tower has been selected from the shop
        if (BuildManager.Instance.SelectedTowerPrefab == null)
        {
            UIManager.Instance.ShowFeedbackMessage("Select a tower to build first!");
            return;
        }

        // Check if the spot is already taken
        if (isOccupied)
        {
            UIManager.Instance.ShowFeedbackMessage("Location is already occupied!");
            return;
        }

        // Check if the player has enough money
        if (GameManager.Instance.SpendMoney(BuildManager.Instance.SelectedTowerCost))
        {
            Instantiate(BuildManager.Instance.SelectedTowerPrefab, transform.position, Quaternion.identity);
            isOccupied = true;
        }
        // No money
        else
        {
            UIManager.Instance.ShowFeedbackMessage("Not enough gold!");
        }
    }
}