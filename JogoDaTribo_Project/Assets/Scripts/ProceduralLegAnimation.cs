using UnityEngine;

/// <summary>
/// Animação Procedural das Pernas do Robô
/// Baseado na hierarquia: Robot > Pernas > Up_Leg.L/R > Leg_Connector > Mid_Leg > Foot / Leg_Middle
/// 
/// Como usar:
/// 1. Adicione este script no GameObject "Robot"
/// 2. Arraste os transforms dos bones nos campos do Inspector
/// 3. Ajuste os parâmetros conforme necessário
/// </summary>
public class ProceduralLegAnimation : MonoBehaviour
{
    [Header("=== BONES DA PERNA ESQUERDA ===")]
    public Transform upLegL;           // Up_Leg.L
    public Transform legConnectorL;    // Leg_Connector_L
    public Transform midLegL;          // Mid_Leg.L
    public Transform footL;            // Foot.L
    public Transform legMiddleL;       // Leg_Middle_L

    [Header("=== BONES DA PERNA DIREITA ===")]
    public Transform upLegR;           // Up_Leg.R
    public Transform legConnectorR;    // Leg_Connector_R
    public Transform midLegR;          // Mid_Leg.R
    public Transform footR;            // Foot.R
    public Transform legMiddleR;       // Leg_Middle_R

    [Header("=== PARÂMETROS DE CAMINHADA ===")]
    [Tooltip("Velocidade do ciclo de caminhada")]
    public float walkCycleSpeed = 2.0f;

    [Tooltip("Altura máxima que o pé levanta ao caminhar")]
    public float stepHeight = 0.3f;

    [Tooltip("Distância que cada passo avança")]
    public float stepLength = 0.4f;

    [Tooltip("Multiplicador geral da amplitude das rotações")]
    public float rotationAmplitude = 1.0f;

    [Header("=== ROTAÇÕES DA COXA (Up_Leg) ===")]
    [Tooltip("Ângulo máximo de balanço da coxa para frente/trás")]
    public float hipSwingAngle = 20f;

    [Tooltip("Ângulo lateral da coxa (abertura/fechamento)")]
    public float hipLateralAngle = 5f;

    [Header("=== ROTAÇÕES DO JOELHO (Mid_Leg) ===")]
    [Tooltip("Ângulo máximo de dobramento do joelho")]
    public float kneeFlexAngle = 30f;

    [Tooltip("Offset de fase do joelho em relação à coxa")]
    public float kneePhaseOffset = 0.3f;

    [Header("=== ROTAÇÕES DO PÉ (Foot) ===")]
    [Tooltip("Ângulo de inclinação do pé ao caminhar")]
    public float footPitchAngle = 15f;

    [Tooltip("Corrige o pé para ficar paralelo ao chão")]
    public bool keepFootLevel = true;

    [Header("=== ANIMAÇÃO IDLE ===")]
    [Tooltip("Habilita animação de idle (quando parado)")]
    public bool enableIdleAnimation = true;

    [Tooltip("Velocidade da oscilação idle")]
    public float idleSpeed = 1.0f;

    [Tooltip("Intensidade da oscilação idle")]
    public float idleAmplitude = 2.0f;

    [Header("=== DETECÇÃO DE MOVIMENTO ===")]
    [Tooltip("Velocidade mínima para considerar que está se movendo")]
    public float movementThreshold = 0.1f;

    [Header("=== DEBUG ===")]
    public bool showDebugGizmos = true;
    public Color gizmoColorL = Color.green;
    public Color gizmoColorR = Color.cyan;

    // ── Variáveis internas ──────────────────────────────────────────────────
    private float _cycleTime;
    private float _idleTime;
    private Vector3 _lastPosition;
    private float _currentSpeed;
    private CharacterController _charController;

    // Rotações originais (salvas no Start para usar como base)
    private Quaternion _upLegL_OrigRot, _upLegR_OrigRot;
    private Quaternion _legConnL_OrigRot, _legConnR_OrigRot;
    private Quaternion _midLegL_OrigRot, _midLegR_OrigRot;
    private Quaternion _footL_OrigRot, _footR_OrigRot;
    private Quaternion _legMidL_OrigRot, _legMidR_OrigRot;

    // ── Propriedades calculadas ─────────────────────────────────────────────
    private bool IsMoving => _currentSpeed > movementThreshold;

    // ═══════════════════════════════════════════════════════════════════════
    void Start()
    {
        _charController = GetComponent<CharacterController>();
        _lastPosition = transform.position;

        // Salva rotações originais de todos os bones
        SaveOriginalRotations();
    }

    // ═══════════════════════════════════════════════════════════════════════
    void Update()
    {
        // Calcula velocidade atual do robô
        CalculateSpeed();

        if (IsMoving)
        {
            // Avança o ciclo de caminhada proporcional à velocidade
            _cycleTime += Time.deltaTime * walkCycleSpeed * (_currentSpeed / Mathf.Max(1f, stepLength));
            AnimateWalk();
        }
        else
        {
            // Retorna suavemente para pose neutra + idle
            ReturnToNeutral();

            if (enableIdleAnimation)
            {
                _idleTime += Time.deltaTime * idleSpeed;
                AnimateIdle();
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>Salva as rotações originais dos bones para uso como base de animação.</summary>
    void SaveOriginalRotations()
    {
        if (upLegL)        _upLegL_OrigRot   = upLegL.localRotation;
        if (upLegR)        _upLegR_OrigRot   = upLegR.localRotation;
        if (legConnectorL) _legConnL_OrigRot  = legConnectorL.localRotation;
        if (legConnectorR) _legConnR_OrigRot  = legConnectorR.localRotation;
        if (midLegL)       _midLegL_OrigRot   = midLegL.localRotation;
        if (midLegR)       _midLegR_OrigRot   = midLegR.localRotation;
        if (footL)         _footL_OrigRot     = footL.localRotation;
        if (footR)         _footR_OrigRot     = footR.localRotation;
        if (legMiddleL)    _legMidL_OrigRot   = legMiddleL.localRotation;
        if (legMiddleR)    _legMidR_OrigRot   = legMiddleR.localRotation;
    }

    // ═══════════════════════════════════════════════════════════════════════
    void CalculateSpeed()
    {
        Vector3 delta = transform.position - _lastPosition;
        delta.y = 0f; // Ignora movimento vertical
        _currentSpeed = delta.magnitude / Time.deltaTime;
        _lastPosition = transform.position;
    }

    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Animação procedural de caminhada.
    /// Perna L e R estão em fases opostas (defasagem de 0.5 no ciclo).
    /// </summary>
    void AnimateWalk()
    {
        // Fase de cada perna: L começa em 0, R começa em 0.5 (opostos)
        float phaseL = _cycleTime;
        float phaseR = _cycleTime + 0.5f;

        AnimateLeg(
            phaseL,
            upLegL, legConnectorL, midLegL, footL, legMiddleL,
            _upLegL_OrigRot, _legConnL_OrigRot, _midLegL_OrigRot, _footL_OrigRot, _legMidL_OrigRot,
            lateralSign: -1f  // Esquerda: inclinação lateral negativa
        );

        AnimateLeg(
            phaseR,
            upLegR, legConnectorR, midLegR, footR, legMiddleR,
            _upLegR_OrigRot, _legConnR_OrigRot, _midLegR_OrigRot, _footR_OrigRot, _legMidR_OrigRot,
            lateralSign: 1f   // Direita: inclinação lateral positiva
        );
    }


    void AnimateLeg(
        float phase,
        Transform upLeg, Transform legConn, Transform midLeg, Transform foot, Transform legMiddle,
        Quaternion origUpLeg, Quaternion origLegConn, Quaternion origMidLeg, Quaternion origFoot, Quaternion origLegMiddle,
        float lateralSign)
    {
        // ── Seno e cosseno da fase atual ────────────────────────────────
        float sinPhase  = Mathf.Sin(phase * Mathf.PI * 2f);
        float cosPhase  = Mathf.Cos(phase * Mathf.PI * 2f);

        // Fase do joelho (levemente atrasada para dobrar antes de levantar o pé)
        float kneePhase = Mathf.Sin((phase - kneePhaseOffset) * Mathf.PI * 2f);
        // Envelope do joelho: só dobra na fase de elevação (valor positivo do seno)
        float kneeEnvelope = Mathf.Max(0f, sinPhase);

        // ── 1. COXA (Up_Leg) ────────────────────────────────────────────
        // Balanço para frente/trás (eixo X local)
        // Inclinação lateral suave (eixo Z local)
        if (upLeg)
        {
            float hipX = sinPhase  * hipSwingAngle   * rotationAmplitude;
            float hipZ = cosPhase  * hipLateralAngle * rotationAmplitude * lateralSign;

            upLeg.localRotation = origUpLeg *
                Quaternion.Euler(hipX, 0f, hipZ);
        }

        // ── 2. CONECTOR DA PERNA (Leg_Connector) ────────────────────────
        // Ajuste fino de rotação no conector para suavizar a transição coxa → canela
        if (legConn)
        {
            float connX = sinPhase * hipSwingAngle * 0.3f * rotationAmplitude;
            legConn.localRotation = origLegConn *
                Quaternion.Euler(connX, 0f, 0f);
        }

        // ── 3. CANELA / JOELHO (Mid_Leg) ────────────────────────────────
        // Dobramento do joelho proporcional ao envelope de elevação
        if (midLeg)
        {
            float kneeX = kneeEnvelope * kneeFlexAngle * rotationAmplitude;
            midLeg.localRotation = origMidLeg *
                Quaternion.Euler(-kneeX, 0f, 0f); // negativo = dobra para trás
        }

        // ── 4. PÉ (Foot) ────────────────────────────────────────────────
        // O pé inclina levemente na direção do movimento
        // Se keepFootLevel estiver ativo, compensa a rotação acumulada da coxa
        if (foot)
        {
            float footX = sinPhase * footPitchAngle * rotationAmplitude;

            if (keepFootLevel && upLeg)
            {
                // Compensa parcialmente a rotação da coxa para o pé ficar nivelado
                float compensation = -sinPhase * hipSwingAngle * 0.6f;
                footX += compensation;
            }

            foot.localRotation = origFoot *
                Quaternion.Euler(footX, 0f, 0f);
        }

        // ── 5. MEIO DA PERNA (Leg_Middle) ────────────────────────────────
        // Ajuste secundário – pequena torção no segmento do meio
        if (legMiddle)
        {
            float midX = kneeEnvelope * kneeFlexAngle * 0.4f * rotationAmplitude;
            legMiddle.localRotation = origLegMiddle *
                Quaternion.Euler(-midX, 0f, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>Animação sutil de idle (robô parado respirando/equilibrando).</summary>
    void AnimateIdle()
    {
        float sinIdle = Mathf.Sin(_idleTime) * idleAmplitude * rotationAmplitude;
        float cosIdle = Mathf.Cos(_idleTime * 0.7f) * idleAmplitude * 0.5f * rotationAmplitude;

        // Oscilação suave e oposta nas duas pernas
        if (upLegL) upLegL.localRotation = _upLegL_OrigRot * Quaternion.Euler(sinIdle, 0f, cosIdle);
        if (upLegR) upLegR.localRotation = _upLegR_OrigRot * Quaternion.Euler(-sinIdle, 0f, -cosIdle);

        if (midLegL) midLegL.localRotation = _midLegL_OrigRot * Quaternion.Euler(Mathf.Abs(sinIdle) * 0.3f, 0f, 0f);
        if (midLegR) midLegR.localRotation = _midLegR_OrigRot * Quaternion.Euler(Mathf.Abs(sinIdle) * 0.3f, 0f, 0f);
    }

    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>Retorna suavemente todos os bones para a pose neutra.</summary>
    void ReturnToNeutral()
    {
        float t = Time.deltaTime * 5f; // Velocidade de retorno

        if (upLegL)        upLegL.localRotation        = Quaternion.Slerp(upLegL.localRotation,        _upLegL_OrigRot,  t);
        if (upLegR)        upLegR.localRotation        = Quaternion.Slerp(upLegR.localRotation,        _upLegR_OrigRot,  t);
        if (legConnectorL) legConnectorL.localRotation = Quaternion.Slerp(legConnectorL.localRotation, _legConnL_OrigRot, t);
        if (legConnectorR) legConnectorR.localRotation = Quaternion.Slerp(legConnectorR.localRotation, _legConnR_OrigRot, t);
        if (midLegL)       midLegL.localRotation       = Quaternion.Slerp(midLegL.localRotation,       _midLegL_OrigRot, t);
        if (midLegR)       midLegR.localRotation       = Quaternion.Slerp(midLegR.localRotation,       _midLegR_OrigRot, t);
        if (footL)         footL.localRotation         = Quaternion.Slerp(footL.localRotation,         _footL_OrigRot,   t);
        if (footR)         footR.localRotation         = Quaternion.Slerp(footR.localRotation,         _footR_OrigRot,   t);
        if (legMiddleL)    legMiddleL.localRotation    = Quaternion.Slerp(legMiddleL.localRotation,    _legMidL_OrigRot, t);
        if (legMiddleR)    legMiddleR.localRotation    = Quaternion.Slerp(legMiddleR.localRotation,    _legMidR_OrigRot, t);
    }

    // ═══════════════════════════════════════════════════════════════════════
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Perna esquerda
        DrawLegGizmos(upLegL, legConnectorL, midLegL, footL, legMiddleL, gizmoColorL);
        // Perna direita
        DrawLegGizmos(upLegR, legConnectorR, midLegR, footR, legMiddleR, gizmoColorR);
    }

    void DrawLegGizmos(Transform upLeg, Transform conn, Transform mid, Transform foot, Transform legMid, Color color)
    {
        Gizmos.color = color;

        if (upLeg && conn)  Gizmos.DrawLine(upLeg.position, conn.position);
        if (conn && mid)    Gizmos.DrawLine(conn.position,  mid.position);
        if (mid && foot)    Gizmos.DrawLine(mid.position,   foot.position);
        if (mid && legMid)  Gizmos.DrawLine(mid.position,   legMid.position);

        // Esferas nos joints
        float sphereSize = 0.04f;
        if (upLeg)  Gizmos.DrawSphere(upLeg.position,  sphereSize);
        if (conn)   Gizmos.DrawSphere(conn.position,   sphereSize);
        if (mid)    Gizmos.DrawSphere(mid.position,    sphereSize);
        if (foot)   Gizmos.DrawSphere(foot.position,   sphereSize * 1.5f);
        if (legMid) Gizmos.DrawSphere(legMid.position, sphereSize);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // API Pública para controle externo (ex: PlayerController, NavMeshAgent)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Força um valor de velocidade externo (útil com NavMeshAgent).</summary>
    public void SetSpeed(float speed)
    {
        _currentSpeed = speed;
    }

    /// <summary>Avança o ciclo de caminhada manualmente.</summary>
    public void AdvanceCycle(float delta)
    {
        _cycleTime += delta;
    }
}
