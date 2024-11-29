using UnityEngine;
using System.Collections;

public class CharacterGraphicsController : MonoBehaviour
{
    private PlayerController m_player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public Sprite defualt;
    public Sprite crouched;
    public Sprite wall;
    public Sprite jump;

    // Use this for initialization
    void Start()
    {
        m_player = GetComponent<PlayerController>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // facing
        switch ( m_player.GetFacingDirection() )
        {
            case PlayerController.FacingDirection.Left:
                spriteRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.Right:
            default:
                spriteRenderer.flipX = false;
                break;
        }

        // sprite
        switch (m_player.GetGraphicsState())
        {
            case PlayerController.GraphicsState.Crouch:
                animator.CrossFade("Crouch Idle",0);
                break;
            case PlayerController.GraphicsState.CrouchWalking:
                animator.CrossFade("Crouch Walk", 0);
                break;
            case PlayerController.GraphicsState.Jump:
                animator.CrossFade("Jump", 0);
                break;
            case PlayerController.GraphicsState.WallSlide:
                animator.CrossFade("Wall Slide", 0);
                break;
            case PlayerController.GraphicsState.Walking:
                animator.CrossFade("Walk", 0);
                break;
            case PlayerController.GraphicsState.Idle:
            default:
                animator.CrossFade("Idle", 0);
                break;
        }
    }
}
