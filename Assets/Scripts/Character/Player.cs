using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    private bool isAllowControl = true;

    public override void Attack()
    {

    }

    private void MovementByJoystick(Vector2 amount)
    {
        var formatAmount = RotateVector2WithCamera(amount);
        Vector3 pos = transform.position;
        pos.x += (formatAmount.x * movementSpeed * Time.fixedDeltaTime);
        pos.z += (formatAmount.y * movementSpeed * Time.fixedDeltaTime);
        transform.position = pos;
        Vector3 movementDirection = new Vector3(formatAmount.x, 0, formatAmount.y);
        if (movementDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        UpdateState(CharacterState.Walking);
    }

    Vector2 RotateVector2WithCamera(Vector2 vector)
    {
        float cameraYRotation = Camera.main.transform.eulerAngles.y;

        float angleInRadians = cameraYRotation * Mathf.Deg2Rad;
        float angle = Mathf.Atan2(vector.y, vector.x) - Mathf.Abs(angleInRadians);
        float radius = vector.magnitude;
        return new Vector2(
            radius * Mathf.Cos(angle),
            radius * Mathf.Sin(angle)
        );
    }
}
