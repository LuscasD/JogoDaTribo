using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [Header("Velocidade")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private bool enableSprint = true;

    [Header("Suavização")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 12f;

    [Header("Rotação")]
    [SerializeField] private bool rotateTowardsMovement = true;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Gravidade")]
    [SerializeField] private float gravity = -20f;

    [Header("Câmera (Opcional)")]
    [Tooltip("Se atribuída, a direção do movimento é relativa à câmera.")]
    [SerializeField] private Camera topDownCamera;

    [Header("Referências (Opcional)")]
    [SerializeField] private Animator animator;

    // Componentes
    private CharacterController controller;

    // Estado interno
    private Vector3 inputDirection;
    private Vector3 currentVelocity;
    private float verticalVelocity;
    private bool isSprinting;

    // Animator hashes
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimMoveX = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveZ = Animator.StringToHash("MoveZ");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (topDownCamera == null)
            topDownCamera = Camera.main;
    }

    private void Update()
    {
        ReadInput();
        ApplyGravity();
        ApplyMovement();
        ApplyRotation();
        UpdateAnimator();
    }

    /// Lê o input do jogador e converte para direção 3D relativa à câmera.
    private void ReadInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");   // W/S

        isSprinting = enableSprint && Input.GetKey(KeyCode.LeftShift);

        // Constrói a direção no plano XZ
        Vector3 rawInput = new Vector3(horizontal, 0f, vertical);

        if (rawInput.sqrMagnitude > 1f)
            rawInput.Normalize();

        // Se houver câmera, torna o movimento relativo à sua orientação (ignora Y)
        if (topDownCamera != null)
        {
            Vector3 camForward = topDownCamera.transform.forward;
            Vector3 camRight = topDownCamera.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            inputDirection = (camForward * rawInput.z + camRight * rawInput.x).normalized * rawInput.magnitude;
        }
        else
        {
            inputDirection = rawInput;
        }
    }

    /// Acumula gravidade quando o personagem está no ar.
    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // pequeno valor para manter o grounding
        else
            verticalVelocity += gravity * Time.deltaTime;
    }

    /// Move o CharacterController com aceleração/desaceleração suave.
    private void ApplyMovement()
    {
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = inputDirection * targetSpeed;

        float lerpFactor = inputDirection.sqrMagnitude > 0f ? acceleration : deceleration;

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            lerpFactor * Time.deltaTime
        );

        // Combina movimento horizontal com gravidade vertical
        Vector3 finalMotion = currentVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalMotion * Time.deltaTime);
    }

    /// Rotaciona o personagem suavemente na direção do movimento.
    private void ApplyRotation()
    {
        if (!rotateTowardsMovement) return;
        if (inputDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    /// Atualiza parâmetros do Animator para blend trees.
    private void UpdateAnimator()
    {
        if (animator == null) return;

        float speed = new Vector3(currentVelocity.x, 0f, currentVelocity.z).magnitude;
        animator.SetFloat(AnimSpeed, speed);

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            animator.SetFloat(AnimMoveX, inputDirection.x);
            animator.SetFloat(AnimMoveZ, inputDirection.z);
        }
    }

    // -------------------------------------------------------
    // API pública
    // -------------------------------------------------------

    /// Aplica um impulso externo no plano horizontal (knockback, explosão, etc).
    public void ApplyKnockback(Vector3 force)
    {
        currentVelocity += new Vector3(force.x, 0f, force.z);
        verticalVelocity += force.y;
    }

   
    /// Para completamente o personagem.
    public void StopMovement()
    {
        inputDirection = Vector3.zero;
        currentVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    /// Direção normalizada do movimento atual.
    public Vector3 GetMoveDirection() => currentVelocity.normalized;

    /// True se o personagem está se movendo.
    public bool IsMoving => currentVelocity.sqrMagnitude > 0.01f;

    /// True se o personagem está em sprint.
    public bool IsSprinting => isSprinting;

    /// True se o personagem está no chão.
    public bool IsGrounded => controller.isGrounded;
}
