using UnityEngine;

/// <summary>
/// Câmera top-down 3D que segue o player e pode ser orbitada ao redor dele.
///
/// Requisitos:
///   - Adicione este script a um GameObject vazio (ex: "CameraRig")
///   - Arraste a Main Camera como filha desse GameObject
///   - Atribua o Transform do player no Inspector
///
/// Controles padrão:
///   - Segurar botão do meio do mouse + arrastar  → orbita ao redor do player
///   - Scroll do mouse                            → zoom in/out
///   - (Opcional) Segurar Alt esquerdo + arrastar → orbita (alternativo)
/// </summary>
public class TopDownCamera3D : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform target;
    [Tooltip("Offset em relação ao pivô do player (útil para centralizar em personagens altos).")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);

    [Header("Distância (Zoom)")]
    [SerializeField] private float currentDistance = 12f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float zoomSmoothing = 8f;

    [Header("Ângulo Vertical (Pitch)")]
    [SerializeField] private float currentPitch = 55f;
    [SerializeField] private float minPitch = 20f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Rotação Horizontal (Yaw)")]
    [SerializeField] private float currentYaw = 0f;

    [Header("Sensibilidade de Órbita")]
    [SerializeField] private float orbitSensitivity = 3f;
    [SerializeField] private float orbitSmoothing = 10f;

    [Header("Seguir Player")]
    [SerializeField] private float followSmoothing = 8f;
    [Tooltip("Se verdadeiro, a câmera rotaciona automaticamente junto com o player.")]
    [SerializeField] private bool autoRotateWithPlayer = false;

    [Header("Colisão com Cenário")]
    [SerializeField] private bool enableCollision = true;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionMask = ~0; // tudo por padrão

    [Header("Teclas de Órbita")]
    [SerializeField] private KeyCode orbitMouseButton = KeyCode.Mouse2;  // botão do meio
    [SerializeField] private KeyCode orbitAltKey      = KeyCode.LeftAlt; // alt + botão esquerdo

    // Estado interno
    private float desiredDistance;
    private float smoothedDistance;
    private float desiredYaw;
    private float desiredPitch;
    private Vector3 smoothedTargetPos;
    private bool isInitialized;

    private void Awake()
    {
        desiredDistance  = currentDistance;
        smoothedDistance = currentDistance;
        desiredYaw       = currentYaw;
        desiredPitch     = currentPitch;
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[TopDownCamera3D] Nenhum alvo atribuído! Atribua o Transform do player no Inspector.");
            return;
        }

        smoothedTargetPos = GetTargetPosition();
        isInitialized = true;

        // Posiciona a câmera imediatamente sem lerp no primeiro frame
        ApplyCameraTransform(snap: true);
    }

    private void LateUpdate()
    {
        if (!isInitialized || target == null) return;

        HandleOrbitInput();
        HandleZoomInput();
        FollowTarget();
        ApplyCameraTransform(snap: false);
    }

    /// <summary>
    /// Lê input de órbita (mouse) e atualiza yaw/pitch desejados.
    /// </summary>
    private void HandleOrbitInput()
    {
        bool orbitingMiddle = Input.GetKey(orbitMouseButton);
        bool orbitingAlt    = Input.GetKey(orbitAltKey) && Input.GetKey(KeyCode.Mouse0);

        if (!orbitingMiddle && !orbitingAlt) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        desiredYaw   += mouseX * orbitSensitivity;
        desiredPitch -= mouseY * orbitSensitivity;
        desiredPitch  = Mathf.Clamp(desiredPitch, minPitch, maxPitch);
    }

    /// <summary>
    /// Lê o scroll do mouse e ajusta a distância desejada.
    /// </summary>
    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        desiredDistance -= scroll * zoomSpeed;
        desiredDistance  = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
    }

    /// <summary>
    /// Suaviza a posição do pivot em direção ao player.
    /// </summary>
    private void FollowTarget()
    {
        Vector3 targetPos = GetTargetPosition();

        if (autoRotateWithPlayer)
            desiredYaw = target.eulerAngles.y;

        smoothedTargetPos = Vector3.Lerp(
            smoothedTargetPos,
            targetPos,
            followSmoothing * Time.deltaTime
        );
    }

    /// <summary>
    /// Calcula e aplica a posição/rotação final da câmera.
    /// </summary>
    private void ApplyCameraTransform(bool snap)
    {
        // Suaviza yaw, pitch e distância
        float t = snap ? 1f : Time.deltaTime;

        currentYaw      = Mathf.LerpAngle(currentYaw,   desiredYaw,   orbitSmoothing * t);
        currentPitch    = Mathf.Lerp(currentPitch,       desiredPitch, orbitSmoothing * t);
        smoothedDistance = Mathf.Lerp(smoothedDistance,  desiredDistance, zoomSmoothing * t);

        // Calcula a posição orbital
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPos  = smoothedTargetPos + rotation * new Vector3(0f, 0f, -smoothedDistance);

        // Colisão: encurta a distância se houver obstáculo entre o pivot e a câmera
        float finalDistance = smoothedDistance;
        if (enableCollision)
            finalDistance = GetCollisionCheckedDistance(rotation);

        Vector3 finalPos = smoothedTargetPos + rotation * new Vector3(0f, 0f, -finalDistance);

        transform.position = finalPos;
        transform.rotation = rotation;
    }

    /// <summary>
    /// Faz um SphereCast do pivot até a posição desejada da câmera para evitar clipping.
    /// </summary>
    private float GetCollisionCheckedDistance(Quaternion rotation)
    {
        Vector3 direction = rotation * Vector3.back; // direção da câmera

        if (Physics.SphereCast(
            smoothedTargetPos,
            collisionRadius,
            direction,
            out RaycastHit hit,
            smoothedDistance,
            collisionMask))
        {
            return Mathf.Max(hit.distance - collisionRadius, minDistance);
        }

        return smoothedDistance;
    }

    /// <summary>
    /// Posição do alvo + offset configurado.
    /// </summary>
    private Vector3 GetTargetPosition()
    {
        return target.position + targetOffset;
    }

    // -------------------------------------------------------
    // API pública
    // -------------------------------------------------------

    /// <summary>
    /// Define um novo alvo para a câmera seguir.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            smoothedTargetPos = GetTargetPosition();
            isInitialized = true;
        }
    }

    /// <summary>
    /// Reseta a câmera para a rotação padrão atrás do player.
    /// </summary>
    public void ResetRotation()
    {
        desiredYaw   = target != null ? target.eulerAngles.y : 0f;
        desiredPitch = 55f;
    }

    /// <summary>
    /// Força a câmera a um yaw/pitch específico (útil em cutscenes).
    /// </summary>
    public void SetRotation(float yaw, float pitch)
    {
        desiredYaw   = yaw;
        desiredPitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    /// <summary>
    /// Retorna a direção "para frente" da câmera no plano XZ (ignora Y).
    /// Útil para o script de movimentação calcular direção relativa à câmera.
    /// </summary>
    public Vector3 GetCameraForwardFlat()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    /// <summary>
    /// Retorna a direção "para a direita" da câmera no plano XZ.
    /// </summary>
    public Vector3 GetCameraRightFlat()
    {
        Vector3 right = transform.right;
        right.y = 0f;
        return right.normalized;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position + targetOffset, 0.25f);
        Gizmos.DrawLine(transform.position, target.position + targetOffset);
    }
#endif
}
