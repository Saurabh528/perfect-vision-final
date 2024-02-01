using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerScoreDestroy : MonoBehaviour
{
    public GameObject particleobject;
    public GameObject particleobject1;

    private Text scoreText;

    public static int score = 0;

    // Use this for initialization
    void Awake()
    {
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        scoreText.text = "0";
    }

    void OnTriggerEnter2D(Collider2D target)
    {

        if (target.tag == "Bomb")
        {
            transform.position = new Vector2(0, 100);
            target.gameObject.SetActive(false);
            //StartCoroutine(RestartGame());
            score--;
        }

        if (target.tag == "Fruits")
        {
           // target.gameObject.SetActive(false);
            score++;
            scoreText.text = "Score:" + score.ToString();
            GameObject e = Instantiate(particleobject1) as GameObject;
            e.transform.position = target.transform.position;
            Destroy(e, 0.5f);
            Destroy(target.gameObject);

            GameObject.Find("Music").GetComponent<Ballon_GameMusic>().GameAudioSource.Play();
        }

        if (target.tag == "Bubble")
        {
           // target.gameObject.SetActive(false);
            score+=1;
            scoreText.text = "Score:" + score.ToString();
            GameObject s = Instantiate(particleobject) as GameObject;
            s.transform.position = target.transform.position;
            Destroy(s, 0.5f);
            Destroy(target.gameObject);
           //   GameObject.Find("Music").GetComponent<GameMusic>().GameAudioSource.Play();  // Will Check Once

        }

    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

} // class

































