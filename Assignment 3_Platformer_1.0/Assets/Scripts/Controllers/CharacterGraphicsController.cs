using UnityEngine;
using System.Collections;

public class CharacterGraphicsController : MonoBehaviour
{
    private PlayerController m_player;
    private SpriteRenderer m_spriteRenderer;

    public Sprite defualt;
    public Sprite crouched;
    public Sprite wall;

    // Use this for initialization
    void Start()
    {
        m_player = GetComponent<PlayerController>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
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

        switch (m_player.GetState())
        {
            case PlayerController.State.Slide:
                m_spriteRenderer.sprite = crouched;
                break;
            case PlayerController.State.WallSlide:
                m_spriteRenderer.sprite = crouched;
                break;
            case PlayerController.State.Default:
            default:
                m_spriteRenderer.sprite = defualt;
                break;
        }
    }
}
