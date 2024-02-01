using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour {

    public Mesh[] meshes;
    public GameObject collisionParticle, tokenParticle;

    private Color[] colors = new Color[2] { Color.red, Color.blue }; // Important color change
    private ParticleSystemRenderer particleRen;
    private TrailRenderer trailRen;
    private Renderer playerRen;
    private MeshFilter playerMesh;
	int _curColorIndex;

	// Use this for initialization
	void Start () {
		colors[0] = ColorCalibration.RedColor;
		colors[1] = ColorCalibration.CyanColor;
		particleRen = GetComponent<ParticleSystemRenderer>();
        trailRen = GetComponent<TrailRenderer>();
        playerRen = GetComponent<Renderer>();
        playerMesh = GetComponent<MeshFilter>();
		_curColorIndex = 0;

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube Instance") || other.CompareTag("Sphere Instance") || other.CompareTag("Prism Instance"))        //If player collides with an obstacle
        {
            if (other.CompareTag(GetComponent<MeshFilter>().mesh.name))      //If the collided gameObject has the same mesh as the player
            {
				if (FindObjectOfType<ShapeChangeController>()._showObstacles)
				{
					_curColorIndex = 1 - _curColorIndex;
                    particleRen.material.color = trailRen.material.color = playerRen.material.color = colors[_curColorIndex];
				}

                FindObjectOfType<Camera>().GetComponent<Animation>().Play();
                playerMesh.mesh = meshes[Random.Range(0, meshes.Length)];       //Changes the player's mesh
                FindObjectOfType<ScoreManager>().IncrementScore();      //Increments score
                FindObjectOfType<AudioManager>().ScoreSound();      //Plays 'scoreSound'
            }
            else        //If the tags are different
            {
                
                FindObjectOfType<AudioManager>().DeathSound();      //Plays 'deathSound'
				if (GameState.currentGamePlay == null)
				{
					EnableCollision(false);
					GamePlayController.Instance.GameOver();
				}
				else
				{
					if (FindObjectOfType<ShapeChangeController>()._showObstacles)
					{
						_curColorIndex = 1 - _curColorIndex;
						particleRen.material.color = trailRen.material.color = playerRen.material.color = colors[_curColorIndex];
					}
					playerMesh.mesh = meshes[Random.Range(0, meshes.Length)];       //Changes the player's mesh
					GamePlayController.Instance.IncreaseScore(-3);
				}
            }
            GameObject tempCollisionParticle = Instantiate(collisionParticle, transform.position, Quaternion.identity);     //Instantiates a collisionParticle to the player's position
            tempCollisionParticle.GetComponent<Renderer>().material.color = playerRen.material.color;       //Sets its color identical to the player's color
            Destroy(tempCollisionParticle, 1.2f);       //Destroys it after x seconds
        }
        else if (other.CompareTag("Token"))
        {
            FindObjectOfType<ScoreManager>().IncrementToken();
            FindObjectOfType<AudioManager>().TokenSound();
            Destroy(Instantiate(tokenParticle, transform.position, Quaternion.identity), 1.2f);
            Destroy(other.gameObject);
        }
    }

	public void EnableCollision(bool enable)
	{
		particleRen.enabled = trailRen.enabled = playerRen.enabled = enable;
		GetComponent<Collider>().enabled = enable;
		FindObjectOfType<JumpManager>().enabled = enable;
	}
}
