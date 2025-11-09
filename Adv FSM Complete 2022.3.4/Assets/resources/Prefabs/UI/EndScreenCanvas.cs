using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// RW
public class EndScreenCanvas : MonoBehaviour
{
    [SerializeField]
    private string winText = "You win!";
    [SerializeField]
    private string loseText = "You lost...";

    [SerializeField]
    private GameObject endScreenContainer;

    [SerializeField]
    private TextMeshProUGUI endScreenText;
    [SerializeField]
    private TextMeshProUGUI winConditionText;


    public void showEndScreen(bool win)
    {
        if (endScreenText != null) {
            string endText = loseText;
            if (win)
            {
                endText = winText;
            }
            endScreenText.text = endText;
        }
    }

    public void toggleEndScreen(bool toggle)
    {
        if (endScreenContainer != null) {
            endScreenContainer.SetActive(toggle);
        }
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void showObjectiveForTime(float seconds)
    {
        StartCoroutine(showObjective(seconds));
    }

    private IEnumerator showObjective(float seconds)
    {
        winConditionText.gameObject.SetActive(true);

        yield return new WaitForSeconds(seconds);

        // translate it up over time (technically should be frame rate independent and it isnt but whatever)
        for (int i = 0; i < 100; i++)
        {
            winConditionText.gameObject.transform.Translate(new Vector3(0, 1, 0));
            yield return null; // wait until next Update frame
        }
    }

}
