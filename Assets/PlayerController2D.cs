using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;

    bool isGrounded;                    //BASIC STATES
    bool isShooting;
    bool isRunning;
    bool isFacingLeft;

    [SerializeField]                    //GROUND CHECKS
    Transform groundCheck;
    [SerializeField]
    Transform groundCheckL;
    [SerializeField]
    Transform groundCheckR;

    [SerializeField]                    //MOVEMENT VARIABLES
    private float runSpeed = 1.5f;
    [SerializeField]
    private float jumpSpeed = 5f;
    [SerializeField]
    private float shotDelay = .5f;

    [SerializeField]                   //DASH VARIABLES
    private float dashSpeed;
    [SerializeField]
    private float dashTime;
    private float startDashTime = 10f;

    [SerializeField]
    private float dashDistance = 10f;
    bool isDashing;

    private float jumpTimeCounter;
    [SerializeField]
    private float jumpTime;

    private bool isJumping;
    private bool doubleJump = false;
    

    [SerializeField]                    //BULLET VARIABLES
    Transform bulletSpawnPos;
    [SerializeField]
    GameObject bullet;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if(Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")) ||                
          (Physics2D.Linecast(transform.position, groundCheckL.position, 1 << LayerMask.NameToLayer("Ground"))) ||
          (Physics2D.Linecast(transform.position, groundCheckR.position, 1 << LayerMask.NameToLayer("Ground"))))
        {
            isGrounded = true;
            bulletSpawnPos.transform.localPosition = new Vector2(bulletSpawnPos.transform.localPosition.x, 0.02f);
        }
        else{
            isGrounded = false;
            if(!Input.GetKey("j")){
                animator.Play("player_jump");
                bulletSpawnPos.transform.localPosition = new Vector2(bulletSpawnPos.transform.localPosition.x, 0.05f);
            }
        }

        if(Input.GetKey("d") || Input.GetKey("right")){                 //MOVING LEFT AND RIGHT
            rb2d.velocity = new Vector2(runSpeed, rb2d.velocity.y);
            isRunning = true;
            isFacingLeft = false;
            bulletSpawnPos.transform.localPosition = new Vector2(0.16f, bulletSpawnPos.transform.localPosition.y);
            if(isGrounded){
                animator.Play("player_run");
                spriteRenderer.flipX = false;
            }
            else{
                animator.Play("player_jump");
                spriteRenderer.flipX = false;
            }
        }
        else if(Input.GetKey("a") || Input.GetKey("left")){
            rb2d.velocity = new Vector2(-runSpeed, rb2d.velocity.y);
            isRunning = true;
            isFacingLeft = true;
            bulletSpawnPos.transform.localPosition = new Vector2(-0.16f, bulletSpawnPos.transform.localPosition.y);
            if(isGrounded){
                animator.Play("player_run");
                spriteRenderer.flipX = true;
            }
            else{
                animator.Play("player_jump");
                spriteRenderer.flipX = true;
                //bulletSpawnPos.transform.position = new Vector2(-bulletSpawnPos.transform.localPosition.x, bulletSpawnPos.transform.position.y);
            }
        }
        else{
            if(isGrounded && !isShooting){
                isRunning = false;
                animator.Play("player_idle");
            }
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }

        /* if(Input.GetKey("k")){
            if(dashTime <= 0){
                Debug.Log("DASH");
                dashTime = startDashTime;
                //rb2d.velocity = Vector2.zero;
            }
            else{
                dashTime -= Time.deltaTime;
                if(isFacingLeft){
                    rb2d.velocity = Vector2.left * dashSpeed;
                }
                else{
                    rb2d.velocity = Vector2.right * dashSpeed;
                }
            }
        } */

        if(Input.GetKey("k") && !isDashing){
            StartCoroutine(Dash());
        }

    }

    private void Update(){
        Debug.Log("doubleJump value: " + doubleJump);

        if((Input.GetKeyDown("space") && isGrounded) || !doubleJump && Input.GetKeyDown("space")){        //Jump
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpSpeed);
            doubleJump = !doubleJump;
            isJumping = true;
            jumpTimeCounter = jumpTime;
            animator.Play("player_jump");
        }
        if(!doubleJump){            //If double jump has already been used, player is considered to be jumping
            isJumping = true;
        }
        if(isGrounded){             //If grounded, reset double jump
            doubleJump = false;
        }
        if(Input.GetKey("space") && isJumping){                             //Double jump
            if(jumpTimeCounter > 0){
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpSpeed);
                jumpTimeCounter -= Time.deltaTime;
            } else{
                isJumping = false;
            }
            animator.Play("player_jump");
        }
        if(Input.GetKeyUp("space")){
            isJumping = false;
        }
        
        //Idle Shooting
        if(Input.GetKey("j") && isGrounded && !isRunning){
            if(isShooting) return;
            isShooting = true;
            animator.Play("player_idle_shoot");

            GameObject b = Instantiate(bullet);
            b.GetComponent<BulletScript>().StartShoot(isFacingLeft);
            b.transform.position = bulletSpawnPos.transform.position;
            Invoke("ResetShoot", shotDelay);
        }
        else if(Input.GetKey("j") && isGrounded && isRunning){
            if(isShooting) return;
            isShooting = true;
            animator.Play("player_run_shoot");
            GameObject b = Instantiate(bullet);
            b.GetComponent<BulletScript>().StartShoot(isFacingLeft);
            b.transform.position = bulletSpawnPos.transform.position;
            Invoke("ResetShoot", shotDelay);
        }

        //Jump Shooting
        if((Input.GetKey("j") && !isGrounded) || (Input.GetKey("j") && isJumping)){
            if(isShooting) return;
            isShooting = true;
            animator.Play("player_jump_shoot");
            GameObject b = Instantiate(bullet);
            b.GetComponent<BulletScript>().StartShoot(isFacingLeft);
            b.transform.position = bulletSpawnPos.transform.position;
            Invoke("ResetShoot", shotDelay);
        }

    }

    void ResetShoot(){
        isShooting = false;
        isRunning = false;
        animator.Play("player_idle");
    }

    IEnumerator Dash(){
        isDashing = true;
        rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
        if(isFacingLeft){
            //rb2d.AddForce(new Vector2(dashDistance * -1f, 0f), ForceMode2D.Impulse);
            rb2d.velocity = new Vector2(-dashSpeed, 0f);
        }
        else{
            //rb2d.AddForce(new Vector2(dashDistance, 0f), ForceMode2D.Impulse);
            rb2d.velocity = new Vector2(dashSpeed, 0f);
        }
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }

}
