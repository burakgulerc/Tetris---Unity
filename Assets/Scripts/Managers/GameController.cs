using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    Board m_gameBoard;

    Spawner m_spawner;

    Shape m_activeShape;
    
    SoundManager m_soundManager;

    public float m_dropInterval = 0.9f;
    float m_dropIntervalModded;
    float m_timeToDrop;

    //float m_timeToNextKey;
    
    //[Range(0.02f, 1f)]
    //public float m_keyRepeatRate = 0.25f;
    
    float m_timeToNextKeyLeftRight;
    
    [Range(0.02f, 1f)]
    public float m_keyRepeatRateLeftRight = 0.15f;
    
    float m_timeToNextKeyDown;
    
    [Range(0.01f, 1f)]
    public float m_keyRepeatRateDown = 0.01f;
    
    float m_timeToNextRotate;
    
    [Range(0.02f, 1f)]
    public float m_keyRepeatRotate = 0.25f;

    bool m_gameOver = false;

    public GameObject m_gameOverPanel;

    // Rotate Toggle Icon

    public IconToggle m_rotIconToggle;

    bool m_clockwise = true;

    // Pause Icon

    public bool m_isPaused = false;
    public GameObject m_pausePanel;

    // Score Manager

    ScoreManager m_scoreManager;

    

    // Start is called before the first frame update
    void Start()
    {
        m_timeToNextKeyLeftRight = Time.time + m_timeToNextKeyLeftRight;
        m_timeToNextRotate = Time.time + m_timeToNextRotate;
        m_timeToNextKeyDown = Time.time + m_timeToNextKeyDown;

        m_gameBoard = GameObject.FindWithTag("Board").GetComponent<Board>();
        m_spawner = GameObject.FindWithTag("Spawner").GetComponent<Spawner>();
        m_soundManager = GameObject.FindObjectOfType<SoundManager>();
        m_scoreManager = GameObject.FindObjectOfType<ScoreManager>();


        if (m_gameBoard is null) Debug.LogWarning("Warning! Game board is not defined.");
        if (m_spawner is null) Debug.LogWarning("Warning! Spawner is not defined.");
        if (m_scoreManager is null) Debug.LogWarning("Warning! Score amanager is not defined.");
        else
        {
            m_spawner.transform.position = Vectorf.Round(m_spawner.transform.position);

            if (!m_activeShape)
            {
                m_activeShape = m_spawner.SpawnShape();
            }
        }

        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(false);
        }

        if(m_pausePanel)
        {
            m_pausePanel.SetActive(false);
        }

        m_dropIntervalModded = m_dropInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_gameBoard || !m_spawner || !m_activeShape || m_gameOver || !m_soundManager || !m_scoreManager) return;

        PlayerInput();

    }

    private void PlayerInput()
    {
        if ((Input.GetButton("MoveRight") && (Time.time > m_timeToNextKeyLeftRight)) || Input.GetButtonDown("MoveRight"))
        {
            m_activeShape.MoveRight();
            m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;


            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                m_activeShape.MoveLeft();
                PlaySound(m_soundManager.m_errorSound,0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }
        }
        else if ((Input.GetButton("MoveLeft") && (Time.time > m_timeToNextKeyLeftRight)) || Input.GetButtonDown("MoveLeft"))
        {
            m_activeShape.MoveLeft();
            m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            { 
                m_activeShape.MoveRight(); 
                PlaySound(m_soundManager.m_errorSound, 0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }



        }
        else if (Input.GetButtonDown("Rotate") && (Time.time > m_timeToNextRotate))
        {
            //m_activeShape.RotateRight();
            m_activeShape.RotateClockwise(m_clockwise);

            m_timeToNextRotate = Time.time + m_keyRepeatRotate;

            if (!m_gameBoard.IsValidPosition(m_activeShape)) 
            {
                //m_activeShape.RotateLeft();
                m_activeShape.RotateClockwise(!m_clockwise);

                PlaySound(m_soundManager.m_errorSound, 0.5f);
            }
            else
            {
                PlaySound(m_soundManager.m_moveSound, 0.5f);
            }

        }
        else if ((Input.GetButton("MoveDown") && (Time.time > m_timeToNextKeyDown)) || (Time.time > m_timeToDrop))
        {
            m_timeToDrop = Time.time + m_dropIntervalModded;
            m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;
            
            m_activeShape.MoveDown();

            if (!m_gameBoard.IsValidPosition(m_activeShape))
            {
                if (m_gameBoard.IsOverLimit(m_activeShape))
                {
                    GameOver();
                }
                else
                {
                    LandShape();
                }

            }
        }
        else if (Input.GetButtonDown("ToggleRot"))
        {
            ToggleRotDirection();
        }
        else if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
    }

    private void PlaySound(AudioClip clip, float volMultiplier)
    {
        if (clip && m_soundManager.m_fxEnabled)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, Mathf.Clamp(m_soundManager.m_fxVolume * volMultiplier,0.05f,1f));
        }
    }

    private void GameOver()
    {
        m_activeShape.MoveUp();
        m_gameOver = true;

        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(true);
        }
        PlaySound(m_soundManager.m_gameOverSound, 2f);
        PlaySound(m_soundManager.m_gameOverVocalClip, 1f);
    }

    private void LandShape()
    {
        m_activeShape.MoveUp();
        m_gameBoard.StoreShapeInGrid(m_activeShape);
        m_activeShape = m_spawner.SpawnShape();

        m_timeToNextKeyLeftRight = Time.time ;
        m_timeToNextRotate = Time.time ;
        m_timeToNextKeyDown = Time.time ;

        m_gameBoard.ClearAllRows();

        PlaySound(m_soundManager.m_dropSound, 0.75f);

        if(m_gameBoard.m_completedRows > 0)
        {
            m_scoreManager.ScoreLines(m_gameBoard.m_completedRows);

            if (m_scoreManager.m_didLevelUp)
            {
                PlaySound(m_soundManager.m_levelUpVocalClip, 0.5f);
                m_dropIntervalModded = Mathf.Clamp(m_dropInterval - (((float)m_scoreManager.m_level - 1) * 0.05f),0.05f,1f);
            }
            else
            {
                if (m_gameBoard.m_completedRows > 1)
                {
                    AudioClip randomVocal = m_soundManager.GetRandomClip(m_soundManager.m_vocalClips);
                    PlaySound(randomVocal, 0.5f);
                }
            }

            

            PlaySound(m_soundManager.m_clearRowSound,0.5f);
        }
    }

    public void Restart()
    {
        Time.timeScale = 1;
        Application.LoadLevel(Application.loadedLevel);
    }

    public void ToggleRotDirection()
    {
        m_clockwise = !m_clockwise;

        if (m_rotIconToggle)
        {
            m_rotIconToggle.ToggleIcon(m_clockwise);
        }
    }

    public void TogglePause()
    {
        if (m_gameOver)
        {
            return;
        }

        m_isPaused = !m_isPaused;

        if (m_pausePanel)
        {
            m_pausePanel.SetActive(m_isPaused);

            if(m_soundManager)
            {
                m_soundManager.m_musicSource.volume = (m_isPaused) ?  m_soundManager.m_musicVolume*0.25f : m_soundManager.m_musicVolume;
            }

            Time.timeScale = (m_isPaused) ? 0 : 1;

        }
    }

}
