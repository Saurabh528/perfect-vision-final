using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class Restart : MonoBehaviour {
	public void RestartGame() {
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
	}
}
