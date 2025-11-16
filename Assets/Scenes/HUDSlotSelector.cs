using UnityEngine;
using UnityEngine.UI;

public class HUDSlotSelector : MonoBehaviour
{
    public Image[] slots;              // 3 slot image
    public Color normalColor = Color.white;
    public Color activeColor = Color.yellow;

    private int activeIndex = 0;

    void Start()
    {
        UpdateSlotVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetActiveSlot(2);
    }

    public void SetActiveSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        activeIndex = index;
        UpdateSlotVisuals();
        Debug.Log($"Slot {index + 1} seçildi!");
    }

    private void UpdateSlotVisuals()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].color = (i == activeIndex) ? activeColor : normalColor;
        }
    }

    public int GetActiveSlotIndex()
    {
        return activeIndex;
    }
}
