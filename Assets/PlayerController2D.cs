using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;

    bool isGrounded;
    bool isShooting;
    bool isFacingLeft;

    [SerializeField]
    Transform groundCheck;
    [SerializeField]
    Transform groundCheckL;
    [SerializeField]
    Transform groundCheckR;

    [SerializeField]
    private float runSpeed = 1.5f;
    [SerializeField]
    private float jumpSpeed = 5f;
    [SerializeField]
    private float shotDelay = .5f;

    [SerializeField]
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
        }
        else{
            isGrounded = false;
            animator.Play("player_jump");
        }

        if(Input.GetKey("d") || Input.GetKey("right")){
            rb2d.velocity = new Vector2(runSpeed, rb2d.velocity.y);
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
            if(isGrounded && !isShooting)
                animator.Play("player_idle");
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }

        if(Input.GetKey("space") && isGrounded){
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpSpeed);
            animator.Play("player_jump");
        }
    }

    private void Update(){
        //Idle Shooting
        if(Input.GetKey("j") && isGrounded){
            if(isShooting) return;
            isShooting = true;
            animator.Play("player_idle_shoot");

            GameObject b = Instantiate(bullet);
            b.GetComponent<BulletScript>().StartShoot(isFacingLeft);
            b.transform.position = bulletSpawnPos.transform.position;
            Invoke("ResetShoot", shotDelay);
        }
    }

    void ResetShoot(){
        isShooting = false;
        animator.Play("player_idle");
    }

}
