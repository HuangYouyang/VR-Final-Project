using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    enum CurrentPlayer
    {
        Player1,
        Player2
    }

    CurrentPlayer currentPlayer;
    bool isGameOver = false;
    bool isWinningShotForPlayer1 = false;
    bool isWinningShotForPlayer2 = false;
    bool ballPocketed = false;
    bool isWaitingForBallMovementToStop = false;
    bool willSwapPlayers = true;
    int player1BallsRemaining = 7;
    int player2BallsRemaining = 7;
    [SerializeField] float shotTimer = 3f;
    [SerializeField] float velThreshold = 0.02f;
    float currentTimer;

    [SerializeField] TextMeshProUGUI player1BallsText;
    [SerializeField] TextMeshProUGUI player2BallsText;
    [SerializeField] TextMeshProUGUI currentTurnText;
    [SerializeField] TextMeshProUGUI messageText;

    [SerializeField] GameObject restartButton;
    [SerializeField] Transform headPosition;
    //[SerializeField] Camera cueStickCamera;
    //[SerializeField] Camera overheadCamera;
    //Camera currentCamera;

    //public Rigidbody cueBall;
    //public float forceMag;

    // Start is called before the first frame update
    void Start()
    {
        currentPlayer = CurrentPlayer.Player1;
        currentTimer = shotTimer;
        //currentCamera = cueStickCamera;
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Vector3 forceDir = Vector3.back;
            cueBall.AddForce(forceDir * forceMag);
            //Vector3 newVelocity = new Vector3(0.0f, 0.0f, -0.05f);
            //cueBall.velocity = newVelocity;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Vector3 forceDir = Vector3.left;
            cueBall.AddForce(forceDir * forceMag);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Vector3 forceDir = Vector3.right;
            cueBall.AddForce(forceDir * forceMag);
        }*/

        if (isWaitingForBallMovementToStop && !isGameOver)
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer > 0)
            {
                return;
            }
            bool allStopped = true;
            foreach (GameObject ball in GameObject.FindGameObjectsWithTag("ball"))
            {
                if (ball.GetComponent<Rigidbody>().velocity.magnitude >= velThreshold)
                {
                    allStopped = false;
                    break;
                }
            }
            if (allStopped)
            {
                isWaitingForBallMovementToStop = false;
                if (willSwapPlayers || !ballPocketed)
                {
                    NextPlayerTurn();
                }
                /*else
                {
                    SwitchCameras();
                }*/
                currentTimer = shotTimer;
                ballPocketed = false;
            }
        }

    }

    void NextPlayerTurn()
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            currentPlayer = CurrentPlayer.Player2;
            currentTurnText.text = "Current Turn: Player 2";
        }
        else
        {
            currentPlayer = CurrentPlayer.Player1;
            currentTurnText.text = "Current Turn: Player 1";
        }
        //willSwapPlayers = false;
        //SwitchCameras();
    }
    /*public void SwitchCameras()
    {
        if (currentCamera == cueStickCamera)
        {
            cueStickCamera.enabled = false;
            overheadCamera.enabled = true;
            currentCamera = overheadCamera;
            isWaitingForBallMovementToStop = true;
        }
        else
        {
            cueStickCamera.enabled = true;
            overheadCamera.enabled = false;
            currentCamera = cueStickCamera;
            //currentCamera.gameObject.GetComponent<CameraController>().ResetCamera();
        }
    }*/
    public void IsShot()
    {
        isWaitingForBallMovementToStop = true;
    }
    public void RestartTheGame()
    {
        SceneManager.LoadScene(0);
    }
    bool Scratch()      // enter cue ball in the final shot
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            if (isWinningShotForPlayer1)
            {
                ScratchOnWinningShot("Player 1");
                return true;
            }
        }
        else
        {
            if (isWinningShotForPlayer2)
            {
                ScratchOnWinningShot("Player 2");
                return true;
            }
        }
        willSwapPlayers = true;
        return false;
    }
    void EarlyEightBall()
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            Lose("Player 1 Hit in the Eight Ball Too Early and Has Lost!");
        }
        else
        {
            Lose("Player 2 Hit in the Eight Ball Too Early and Has Lost!");
        }
    }
    void ScratchOnWinningShot(string player)
    {
        Lose(player + " Scratched on Their Final Shot and Has Lost!");
    }
    bool CheckBall(Ball ball)
    {
        if (ball.IsCueBall())
        {
            if (Scratch())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (ball.IsEightBall())
        {
            if (currentPlayer == CurrentPlayer.Player1)
            {
                if (isWinningShotForPlayer1)
                {
                    Win("Player 1");
                    return true;
                }
            }
            else
            {
                if (isWinningShotForPlayer2)
                {
                    Win("Player 2");
                    return true;
                }
            }
            EarlyEightBall();
        }
        else
        {
            if (ball.IsBallRed())
            {
                player1BallsRemaining--;
                player1BallsText.text = "Player 1 Balls Remaining: " + player1BallsRemaining;
                if (player1BallsRemaining <= 0)
                {
                    isWinningShotForPlayer1 = true;
                }
                if (currentPlayer != CurrentPlayer.Player1)
                {
                    willSwapPlayers = true;
                }
                else
                {
                    willSwapPlayers = false;
                }
            }
            else
            {
                player2BallsRemaining--;
                player2BallsText.text = "Player 2 Balls Remaining: " + player2BallsRemaining;
                if (player2BallsRemaining <= 0)
                {
                    isWinningShotForPlayer2 = true;
                }
                if (currentPlayer != CurrentPlayer.Player2)
                {
                    willSwapPlayers = true;
                }
                else
                {
                    willSwapPlayers = false;
                }
            }
        }
        return true;
    }
    void Lose(string message)
    {
        isGameOver = true;
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        restartButton.SetActive(true);
    }
    void Win(string player)
    {
        isGameOver = true;
        messageText.gameObject.SetActive(true);
        messageText.text = player + " Has Won!";
        restartButton.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.tag == "ball")
        {
            //currentTurnText.text = "Fall";
            ballPocketed = true;
            if (CheckBall(other.gameObject.GetComponent<Ball>()))
            {
                Destroy(other.gameObject);
            }
            else
            {
                //currentTurnText.text = "White Fall";
                other.gameObject.transform.position = headPosition.position;
                other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }
    }
}
