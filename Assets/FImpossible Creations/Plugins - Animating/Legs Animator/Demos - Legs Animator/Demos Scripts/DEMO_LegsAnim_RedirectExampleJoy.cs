using FIMSpace.Basics;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

public class DEMO_LegsAnim_RedirectExampleJoy : MonoBehaviour
{
    public LegsAnimator Legs;
    public Fimp_JoystickInput Joystick;

    public bool DebugWSAD = false;
    public Vector2 ConstantDebugInputVal = Vector2.zero;

    [Range(0f,1f)] public float ModuleBlend = 1f;
    LAM_DirectionalMovement module;

    private void Start()
    {
        module = Legs.GetModule<LAM_DirectionalMovement>();
    }

    void Update()
    {
        UpdateInputs();
        Legs.User_SetIsMoving(Legs.DesiredMovementDirection.magnitude > 0f);
        module.ModuleBlend = ModuleBlend;
    }

    void UpdateInputs()
    {
        if (ConstantDebugInputVal != Vector2.zero)
        {
            Legs.User_SetDesiredMovementDirection(new Vector3(ConstantDebugInputVal.x, 0, ConstantDebugInputVal.y).normalized);
            return;
        }
        
        if (DebugWSAD)
        {
            Vector2 dir = Vector2.zero;
            if (Input.GetKey(KeyCode.W)) dir += Vector2.up;
            if (Input.GetKey(KeyCode.S)) dir += Vector2.down;
            if (Input.GetKey(KeyCode.A)) dir += Vector2.left;
            if (Input.GetKey(KeyCode.D)) dir += Vector2.right;

            dir.Normalize();
            Legs.User_SetDesiredMovementDirection(new Vector3(dir.x, 0, dir.y));

            if (dir != Vector2.zero) return;
        }

        Legs.User_SetDesiredMovementDirection(new Vector3(Joystick.OutputValue.x, 0, Joystick.OutputValue.y));
    }

}
