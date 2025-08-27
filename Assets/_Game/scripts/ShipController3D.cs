using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ShipController3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Forward Motion")]
    [SerializeField] private float forwardSpeed = 40f;
    [SerializeField] private float boostMultiplier = 1.8f;
    [SerializeField] private float brakeMultiplier = 0.55f;

    [Header("Screen-Space Movement")]
    [SerializeField] private float lateralSpeed = 22f;   // esquerda/direita
    [SerializeField] private float verticalSpeed = 18f;  // cima/baixo
    [SerializeField] private float positionSmoothTime = 0.08f;
    [SerializeField, Range(0.05f, 0.5f)] private float viewportPaddingX = 0.1f;
    [SerializeField, Range(0.05f, 0.5f)] private float viewportPaddingY = 0.1f;

    [Header("Tilt / Rotation")]
    [SerializeField] private float maxBankAngle = 35f;    // rolagem (Z) quando vai p/ lados
    [SerializeField] private float maxPitchAngle = 15f;   // inclinação (X) quando sobe/desce
    [SerializeField] private float rotationLerpSpeed = 10f;

    [Header("Barrel Roll")]
    [SerializeField] private float rollDuration = 0.45f;
    private float rollOffset; // 0..360 aplicado no eixo da câmera
    private bool isRolling;

    private Vector3 posVelocity; // p/ SmoothDamp
    private float currentForwardSpeed;

#if ENABLE_INPUT_SYSTEM
    private InputAction moveAction;
    private InputAction boostAction;
    private InputAction brakeAction;
    private InputAction rollLeftAction;
    private InputAction rollRightAction;
#endif

    private void Reset()
    {
        if (!mainCamera) mainCamera = Camera.main;
    }

    private void Awake()
    {
        if (!mainCamera) mainCamera = Camera.main;

        currentForwardSpeed = forwardSpeed;

#if ENABLE_INPUT_SYSTEM
        // MAPAS DE INPUT (construídos por código para ficar plug & play)
        moveAction = new InputAction("Move");
        // Teclado (WASD/Setas) como 2D Composite
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
        // Gamepad Stick
        moveAction.AddBinding("<Gamepad>/leftStick");

        boostAction = new InputAction("Boost");
        boostAction.AddBinding("<Keyboard>/leftShift");
        boostAction.AddBinding("<Gamepad>/rightTrigger");

        brakeAction = new InputAction("Brake");
        brakeAction.AddBinding("<Keyboard>/leftCtrl");
        brakeAction.AddBinding("<Gamepad>/leftTrigger");

        rollLeftAction = new InputAction("RollLeft");
        rollLeftAction.AddBinding("<Keyboard>/q");
        rollLeftAction.AddBinding("<Gamepad>/leftShoulder");

        rollRightAction = new InputAction("RollRight");
        rollRightAction.AddBinding("<Keyboard>/e");
        rollRightAction.AddBinding("<Gamepad>/rightShoulder");
#endif
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        moveAction?.Enable();
        boostAction?.Enable();
        brakeAction?.Enable();
        rollLeftAction?.Enable();
        rollRightAction?.Enable();

        if (rollLeftAction != null) rollLeftAction.performed += _ => TryBarrelRoll(-1f);
        if (rollRightAction != null) rollRightAction.performed += _ => TryBarrelRoll(+1f);
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (rollLeftAction != null) rollLeftAction.performed -= _ => TryBarrelRoll(-1f);
        if (rollRightAction != null) rollRightAction.performed -= _ => TryBarrelRoll(+1f);

        moveAction?.Disable();
        boostAction?.Disable();
        brakeAction?.Disable();
        rollLeftAction?.Disable();
        rollRightAction?.Disable();
#endif
    }

    void Update()
    {
        if (!mainCamera) return;

        // 1) Ler input 2D
        Vector2 input = ReadMoveInput();
        input = Vector2.ClampMagnitude(input, 1f);

        // 2) Velocidade para frente com boost/brake
        float speed = forwardSpeed;
        if (IsBoosting()) speed *= boostMultiplier;
        if (IsBraking()) speed *= brakeMultiplier;
        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, speed, 3f * Time.deltaTime);

        // 3) Calcular destino em mundo: para frente + deslocamentos lateral/vertical relativos à câmera
        Vector3 desired = transform.position;
        desired += mainCamera.transform.forward * (currentForwardSpeed * Time.deltaTime);
        desired += mainCamera.transform.right * (input.x * lateralSpeed * Time.deltaTime);
        desired += mainCamera.transform.up * (input.y * verticalSpeed * Time.deltaTime);

        // 4) "Clamp" dentro da tela (viewport), respeitando o z atual (distância à câmera)
        float depth = Vector3.Dot(desired - mainCamera.transform.position, mainCamera.transform.forward);
        Vector3 v = mainCamera.WorldToViewportPoint(desired);

        float minX = viewportPaddingX;
        float maxX = 1f - viewportPaddingX;
        float minY = viewportPaddingY;
        float maxY = 1f - viewportPaddingY;

        v.x = Mathf.Clamp(v.x, minX, maxX);
        v.y = Mathf.Clamp(v.y, minY, maxY);

        Vector3 clampedWorld = mainCamera.ViewportToWorldPoint(new Vector3(v.x, v.y, depth));

        // 5) Suavizar posição
        transform.position = Vector3.SmoothDamp(transform.position, clampedWorld, ref posVelocity, positionSmoothTime);

        // 6) Rotação desejada (facing + tilt + barrel roll)
        Quaternion facing = Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);

        // Tilt por input
        float bank = -input.x * maxBankAngle;   // rola para o lado ao mover lateralmente
        float pitch = input.y * maxPitchAngle;  // inclina ao subir/descer

        Quaternion tilt =
            Quaternion.AngleAxis(pitch, mainCamera.transform.right) *
            Quaternion.AngleAxis(bank, mainCamera.transform.forward);

        // Offset de Barrel Roll (animado por coroutine)
        Quaternion roll = Quaternion.AngleAxis(rollOffset, mainCamera.transform.forward);

        Quaternion targetRot = roll * tilt * facing;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);
    }

    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (moveAction != null)
            return moveAction.ReadValue<Vector2>();
#endif
        // Fallback simples com Input Manager antigo:
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        return new Vector2(x, y);
    }

    private bool IsBoosting()
    {
#if ENABLE_INPUT_SYSTEM
        if (boostAction != null) return boostAction.IsPressed();
#endif
        return Input.GetKey(KeyCode.LeftShift);
    }

    private bool IsBraking()
    {
#if ENABLE_INPUT_SYSTEM
        if (brakeAction != null) return brakeAction.IsPressed();
#endif
        return Input.GetKey(KeyCode.LeftControl);
    }

    private void TryBarrelRoll(float dir)
    {
        if (!isRolling) StartCoroutine(BarrelRoll(dir));
    }

    private System.Collections.IEnumerator BarrelRoll(float direction)
    {
        isRolling = true;
        rollOffset = 0f;
        float t = 0f;

        // Curva de velocidade suave (acelera e desacelera)
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        while (t < rollDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / rollDuration);
            float eased = curve.Evaluate(k);
            rollOffset = direction * Mathf.Lerp(0f, 360f, eased);
            yield return null;
        }

        rollOffset = 0f;
        isRolling = false;
    }
}
