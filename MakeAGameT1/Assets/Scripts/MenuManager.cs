using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadGame()
    {
        SceneManager.LoadScene("MainGame");
    }
    public void QuitGame()
    {
        Application.Quit(); 
    }
    public void SwitchToMenu(){
        SceneManager.LoadScene("Menu");
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        SceneManager.LoadScene("WinScreen");
    }
}
