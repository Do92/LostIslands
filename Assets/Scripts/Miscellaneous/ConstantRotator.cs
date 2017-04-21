using UnityEngine;

namespace Miscellaneous
{
    public class ConstantRotator : MonoBehaviour
    {
        [Tooltip("The rotational speed")]
        public int degreesPerSecond = 50;

        public bool invertDirection = false;

        // Update is called once per frame
        private void Update()
        {
            // Rotates the specified amount of degrees per second around z axis
            transform.Rotate(0, 0, (invertDirection ? -degreesPerSecond : degreesPerSecond) * Time.deltaTime);
        }
    }
}