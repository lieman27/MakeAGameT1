using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{

    public float Health;
    public float MaxHealth;

    public float width; 
    public float height;

    
    [SerializeField]
    private RectTransform healthBar;

    public void SetMaxHealth(float maxHealth)
    {
        MaxHealth = maxHealth;
    }

        public void SetHealth(float health)
    {
        Health = health;
        float newWidth = Health / MaxHealth * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
    }

}
