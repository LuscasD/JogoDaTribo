using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Rotação do Torso")]
    [SerializeField] private Transform body;
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float bodyHeightOffset = 1f;

    [Header("Feedback de Hit")]
    [SerializeField] private float rotationFreezeOnHit = 0.15f; // segundos que a rotação para ao acertar
    [SerializeField] private float hitBumpSpeed = 60f;           // graus/s de "soluço" ao acertar

    // ── Referências internas ──────────────────────────────────────
    private Camera _cam;
    private SawAttack _sawAttack;

    // ── Estado ───────────────────────────────────────────────────
    private float _freezeUntil;
    private bool _bumpActive;
    private float _bumpSign = 1f;

    // ── API pública (chamada pelo SawAttack) ──────────────────────
    public void OnSawHit()
    {
        _freezeUntil = Time.time + rotationFreezeOnHit;
        _bumpActive = true;
        _bumpSign = Random.value > 0.5f ? 1f : -1f; // soluço aleatório
    }

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _cam = Camera.main;
        _sawAttack = GetComponent<SawAttack>();
    }

    private void Update()
    {
        if (Time.time < _freezeUntil)
        {
            ApplyHitBump();
        }
        else
        {
            _bumpActive = false;
            RotateBodyTowardsMouse();
        }
    }

    // ─────────────────────────────────────────────────────────────
    private void RotateBodyTowardsMouse()
    {
        if (body == null || _cam == null) return;

        Plane bodyPlane = new Plane(Vector3.up,
            new Vector3(0f, transform.position.y + bodyHeightOffset, 0f));

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (bodyPlane.Raycast(ray, out float dist))
        {
            Vector3 worldPoint = ray.GetPoint(dist);
            Vector3 direction = worldPoint - body.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
            body.rotation = Quaternion.Slerp(body.rotation, target,
                                             rotateSpeed * Time.deltaTime);
        }
    }

    /// Pequeno "soluço" de rotação no momento do impacto — dá sensação de recuo.
    private void ApplyHitBump()
    {
        if (!_bumpActive || body == null) return;
        body.Rotate(Vector3.up, _bumpSign * hitBumpSpeed * Time.deltaTime, Space.World);
    }
}

