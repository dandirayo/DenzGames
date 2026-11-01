using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveController : MonoBehaviour
{
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;
    public float stopDecel = 14f;
    public bool flipCharacterWithDirection = true;

    [Header("Difficulty Scaling")]
    [Tooltip("Kecepatan maksimum bertambah sebesar ini tiap 'difficultyScoreStep' poin skor.")]
    public float speedIncreasePerStep = 0.5f;
    public int difficultyScoreStep = 100;
    public float absoluteMaxSpeed = 20f;

    private Rigidbody2D rig;
    private Animator anim;
    private CharacterSoundController sound;
    private Vector3 originalScale;

    [Header("Jump")]
    public float jumpAccel;
    // FITUR: double jump (set extraJumps = 0 untuk perilaku single-jump lama)
    public int extraJumps = 1;
    // EFISIENSI/UX: input buffer supaya klik sesaat sebelum mendarat tetap terhitung
    public float jumpBufferTime = 0.12f;
    // UX: coyote time supaya lompat sesaat setelah lepas dari tepi tetap bisa
    public float coyoteTime = 0.1f;

    private bool isJumping;
    private bool isOnGround;
    private int jumpsRemaining;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private float horizontalInput;

    private float lastPositionX;

    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;


    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();
        originalScale = transform.localScale;

        lastPositionX = transform.position.x;
        jumpsRemaining = extraJumps;
        baseMaxSpeedCache = maxSpeed;
        currentMaxSpeed = maxSpeed;
    }


    private void Update()
    {
        // FITUR: game bisa di-pause lewat GameManager (Time.timeScale = 0),
        // input lompat diabaikan selagi pause supaya tidak "tertelan"
        if (GameManager.IsPaused) return;

        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) horizontalInput += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput -= 1f;

        if (flipCharacterWithDirection && horizontalInput != 0f)
        {
            Vector3 newScale = originalScale;
            newScale.x = Mathf.Abs(originalScale.x) * Mathf.Sign(horizontalInput);
            transform.localScale = newScale;
        }

        // input jump: mouse klik ATAU keyboard (Space) ATAU tap layar (mobile)
        bool jumpPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);

        if (jumpPressed)
        {
            jumpBufferTimer = jumpBufferTime; // simpan input, dieksekusi begitu boleh lompat
        }
        else if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (isOnGround)
        {
            coyoteTimer = coyoteTime;
        }
        else if (coyoteTimer > 0f)
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f)
        {
            bool canGroundJump = isOnGround || coyoteTimer > 0f;
            bool canAirJump = !canGroundJump && jumpsRemaining > 0;

            if (canGroundJump)
            {
                DoJump();
                jumpsRemaining = extraJumps;
            }
            else if (canAirJump)
            {
                DoJump();
                jumpsRemaining--;
            }
        }

        //mengganti Animasi - change Animation
        anim.SetBool("isOnGround", isOnGround);
        bool isIdleOnGround = isOnGround && Mathf.Abs(horizontalInput) < 0.01f;
        if (isIdleOnGround)
        {
            anim.Play("characterMove", 0, 0f);
            anim.speed = 0f;
        }
        else
        {
            anim.speed = 1f;
        }

        //calculate score
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if(scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }

        // FITUR: difficulty scaling - makin tinggi skor, makin cepat karakter
        // (baseMaxSpeedCache diisi sekali dari nilai Inspector di FixedUpdate pertama)
        if (baseMaxSpeedCache >= 0f)
        {
            float bonusSpeed = Mathf.Floor(score.GetCurrentScore() / difficultyScoreStep) * speedIncreasePerStep;
            currentMaxSpeed = Mathf.Min(absoluteMaxSpeed, baseMaxSpeedCache + bonusSpeed);
        }

        // FITUR: efek slow-down dari "Polisi Tidur" (speed bump)
        if (slowDownTimer > 0f)
        {
            slowDownTimer -= Time.deltaTime;
            if (slowDownTimer <= 0f) slowDownFactor = 1f;
        }

        //game over
        if (transform.position.y < fallPositionY)
        {
            GameOver();
        }

    }

    private float baseMaxSpeedCache = -1f;
    private float currentMaxSpeed;

    // FITUR: dipakai obstacle "Polisi Tidur" (speed bump) - melambatkan
    // karakter sesaat, alternatif yang lebih ramah daripada instant game over
    private float slowDownTimer;
    private float slowDownFactor = 1f;

    public void ApplySlowDown(float duration, float speedFactor)
    {
        slowDownTimer = duration;
        slowDownFactor = speedFactor;
    }

    private void DoJump()

    {
        isJumping = true;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        sound.PlayJump();
    }

    private void FixedUpdate()
    {
        //ini Raycast ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit)
        {
            if (!isOnGround && rig.velocity.y <= 0)
            {
                isOnGround = true;
            }
        }
        else
        {
            isOnGround = false;
        }
        // calculate velocity vector
        Vector2 velocityVector = rig.velocity;

        if(isJumping)
        {

            velocityVector.y += jumpAccel;
            isJumping = false;

        }
        float activeMaxSpeed = currentMaxSpeed * slowDownFactor;
        if (Mathf.Abs(horizontalInput) > 0f)
        {
            float targetSpeed = horizontalInput * activeMaxSpeed;
            velocityVector.x = Mathf.MoveTowards(velocityVector.x, targetSpeed, moveAccel * Time.fixedDeltaTime);
        }
        else
        {
            velocityVector.x = Mathf.MoveTowards(velocityVector.x, 0f, stopDecel * Time.fixedDeltaTime);
        }

        rig.velocity = velocityVector;
    }

    // FITUR: dipanggil ObstacleController saat karakter menabrak rintangan
    public void HitObstacle()
    {
        if (!this.enabled) return; // sudah game over, abaikan tabrakan susulan
        GameOver();
    }

    private void GameOver()
    {
        //Set High Score
        score.FinishScoring();

        //stop camera movement
        gameCamera.enabled = false;

        //show
        gameOverScreen.SetActive(true);

        //disable
        this.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.white);
    }

}
