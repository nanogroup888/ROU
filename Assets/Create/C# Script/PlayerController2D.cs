using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการเคลื่อนที่แนวนอน")]
    public float moveSpeed = 5f;

    [Tooltip("แรงกระโดด (ตั้งค่าเป็นความเร็วแกน Y ตอนกดกระโดด)")]
    public float jumpForce = 10f;

    [Header("Ground Check")]
    [Tooltip("จุดตรวจสอบพื้น ให้วางไว้ใต้เท้าตัวละครเล็กน้อย")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    private bool warnedNoGroundCheck = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            UnityEngine.Debug.LogError("PlayerController2D: ต้องมี Rigidbody2D บน GameObject นี้");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        // กันค่าพารามิเตอร์เพี้ยน
        if (float.IsNaN(moveSpeed) || float.IsInfinity(moveSpeed)) moveSpeed = 5f;
        if (float.IsNaN(jumpForce) || float.IsInfinity(jumpForce)) jumpForce = 10f;
        if (float.IsNaN(groundCheckRadius) || groundCheckRadius <= 0f) groundCheckRadius = 0.2f;

        // รีเซ็ต velocity ถ้าเพี้ยน
        if (IsBad(rb.velocity)) rb.velocity = Vector2.zero;
    }

    void Update()
    {
        // รับอินพุตแนวนอน
        moveInput = Input.GetAxisRaw("Horizontal");
        if (float.IsNaN(moveInput) || float.IsInfinity(moveInput)) moveInput = 0f;

        // กระโดดเมื่อแตะพื้น
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector2 v = rb.velocity;
            if (IsBad(v)) v = Vector2.zero;
            v.y = jumpForce;
            rb.velocity = Sanitize(v);
        }
    }

    void FixedUpdate()
    {
        // อัปเดตสถานะแตะพื้น
        if (groundCheck != null)
        {
            Vector3 gp3 = groundCheck.position;
            if (IsBad(gp3)) // ถ้า groundCheck โดนสคริปต์อื่นพาไป NaN ให้ถือว่ายังไม่แตะพื้น
            {
                isGrounded = false;
            }
            else
            {
                isGrounded = Physics2D.OverlapCircle((Vector2)gp3, groundCheckRadius, groundLayer);
            }
        }
        else
        {
            if (!warnedNoGroundCheck)
            {
                UnityEngine.Debug.LogWarning("PlayerController2D: ยังไม่ได้ตั้งค่า groundCheck ใน Inspector");
                warnedNoGroundCheck = true;
            }
            isGrounded = false;
        }

        // เคลื่อนที่แนวนอน
        Vector2 current = rb.velocity;
        if (IsBad(current)) current = Vector2.zero;

        Vector2 target = new Vector2(moveInput * moveSpeed, current.y);
        rb.velocity = Sanitize(target);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // ---------- Utilities ----------
    static bool IsBad(Vector2 v) =>
        float.IsNaN(v.x) || float.IsNaN(v.y) ||
        float.IsInfinity(v.x) || float.IsInfinity(v.y);

    static bool IsBad(Vector3 v) =>
        float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
        float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);

    static Vector2 Sanitize(Vector2 v)
    {
        if (float.IsNaN(v.x) || float.IsInfinity(v.x)) v.x = 0f;
        if (float.IsNaN(v.y) || float.IsInfinity(v.y)) v.y = 0f;
        return v;
    }
}
