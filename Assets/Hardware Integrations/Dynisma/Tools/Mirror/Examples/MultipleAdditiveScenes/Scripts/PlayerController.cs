using UnityEngine;
using UnityEngine.SceneManagement;
using VersionCompatibility;

namespace Mirror.Examples.MultipleAdditiveScenes
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour
    {
        public CharacterController characterController;

        void OnValidate()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            characterController.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().clientAuthority = true;
        }

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;
        }

        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float turnSensitivity = 5f;
        public float maxTurnSpeed = 100f;

        [Header("Diagnostics")]
        public float horizontal;
        public float vertical;
        public float turn;
        public float jumpSpeed;
        public bool isGrounded = true;
        public bool isFalling;
        public Vector3 velocity;

        void Update()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            horizontal = UnityInput.GetAxis(UnityAxis.Horizontal);
            vertical = UnityInput.GetAxis(UnityAxis.Vertical);

            // Q and E cancel each other out, reducing the turn to zero
            if (UnityInput.GetKey(UnityKey.Q))
                turn = Mathf.MoveTowards(turn, -maxTurnSpeed, turnSensitivity);
            if (UnityInput.GetKey(UnityKey.E))
                turn = Mathf.MoveTowards(turn, maxTurnSpeed, turnSensitivity);
            if (UnityInput.GetKey(UnityKey.Q) && UnityInput.GetKey(UnityKey.E))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
            if (!UnityInput.GetKey(UnityKey.Q) && !UnityInput.GetKey(UnityKey.E))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);

            if (isGrounded)
                isFalling = false;

            if ((isGrounded || !isFalling) && jumpSpeed < 1f && UnityInput.GetKey(UnityKey.Space))
            {
                jumpSpeed = Mathf.Lerp(jumpSpeed, 1f, 0.5f);
            }
            else if (!isGrounded)
            {
                isFalling = true;
                jumpSpeed = 0;
            }
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);

            Vector3 direction = new Vector3(horizontal, jumpSpeed, vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;

            if (jumpSpeed > 0)
                characterController.Move(direction * Time.fixedDeltaTime);
            else
                characterController.SimpleMove(direction);

            isGrounded = characterController.isGrounded;
            velocity = characterController.velocity;
        }
    }
}
