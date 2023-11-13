using UnityEngine;
using System.Collections;

public class CharacterGraphicsController : MonoBehaviour
{
    private PlayerController m_player;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private int IsWalkingProperty;
    private int IsGroundedProperty;

    // Use this for initialization
    void Start()
    {
        m_player = GetComponent<PlayerController>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        //m_animator = GetComponentInChildren<Animator>();

        //IsWalkingProperty = Animator.StringToHash( "IsWalking" );
        //IsGroundedProperty = Animator.StringToHash( "IsGrounded" );
    }

    // Update is called once per frame
    void Update()
    {
        //m_animator.SetBool( IsWalkingProperty, m_player.IsWalking() );
        //m_animator.SetBool( IsGroundedProperty, m_player.IsGrounded() );
        switch ( m_player.GetFacingDirection() )
        {
            case PlayerController.FacingDirection.Left:
                m_spriteRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.Right:
            default:
                m_spriteRenderer.flipX = false;
                break;
        }
    }
}
