using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<GameObject> paintings;

    public GameObject currentPainting;
    public GameObject lastPainting;

    [SerializeField]
    private MainManager mainManager;
    private int index;

    // Ui changes here
    public GameObject nextBtn;
    public GameObject doneBtn;

    public float animTime = 2f;
    public GameObject confettiEffect;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;

        ShowNextPainting();
    }

    // Both side buttons management is done here
    public void OnClickDone()
    {
        doneBtn.SetActive(false);

        // Start the Animation of the current frame
        currentPainting.GetComponent<Animation>().Play();
        confettiEffect.SetActive(true);

        StartCoroutine(AfterAnimation());
    }
    public void OnClickNext()
    {
        nextBtn.SetActive(false);

        ShowNextPainting();
        doneBtn.SetActive(true);
    }

    IEnumerator AfterAnimation()
    {
        yield return new WaitForSeconds(animTime);

        nextBtn.SetActive(true);
        confettiEffect.SetActive(false);
    }

    void ShowNextPainting()
    {
        if(index < paintings.Count)
        {
            currentPainting = paintings[index];
            lastPainting = (index == 0) ? null : paintings[index - 1];
            
            currentPainting.SetActive(true);
            mainManager.framesManager = currentPainting.GetComponent<FramesManager>();

            if(lastPainting)
            lastPainting.SetActive(false);

            index++;
        }
        else
        {
            Debug.Log("No Painting to show");
            SceneManager.LoadScene(0);
        }
    }
}
