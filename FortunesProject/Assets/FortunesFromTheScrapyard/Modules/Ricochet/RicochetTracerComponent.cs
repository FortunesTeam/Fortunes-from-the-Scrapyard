using UnityEngine;
using RoR2;
using UnityEngine.Events;


namespace FortunesFromTheScrapyard.Ricochet
{
    public class RicochetTracer : MonoBehaviour
    {
        [Tooltip("A child transform which will be placed at the start of the tracer path upon creation.")]
        public Transform startTransform;

        [Tooltip("Child object to scale to the length of this tracer and burst particles on based on that length. Optional.")]
        public GameObject beamObject;

        [Tooltip("The number of particles to emit per meter of length if using a beam object.")]
        public float beamDensity = 10f;

        [Tooltip("The travel speed of this tracer.")]
        public float speed = 1f;

        [Tooltip("Child transform which will be moved to the head of the tracer.")]
        public Transform headTransform;

        [Tooltip("Child transform which will be moved to the tail of the tracer.")]
        public Transform tailTransform;

        [Tooltip("The maximum distance between head and tail transforms.")]
        public float length = 1f;

        [Tooltip("Reverses the travel direction of the tracer.")]
        public bool reverse;

        [Tooltip("The event that runs when the tail reaches the destination.")]
        public UnityEvent onTailReachedDestination;

        private Vector3 startPos;

        private Vector3 endPos;

        private float distanceTraveled;

        private float totalDistance;

        private Vector3 normal;

        private EventFunctions eventFunctions;

        private void Start()
        {
            eventFunctions = GetComponent<EventFunctions>();
            EffectComponent component = GetComponent<EffectComponent>();
            endPos = component.effectData.origin;
            Transform transform = component.effectData.ResolveChildLocatorTransformReference();
            startPos = (transform ? transform.position : component.effectData.start);
            if (reverse)
            {
                Util.Swap(ref endPos, ref startPos);
            }
            Vector3 vector = endPos - startPos;
            distanceTraveled = 0f;
            totalDistance = Vector3.Magnitude(vector);
            if (totalDistance != 0f)
            {
                normal = vector * (1f / totalDistance);
                base.transform.rotation = Util.QuaternionSafeLookRotation(normal);
            }
            else
            {
                normal = Vector3.zero;
            }
            if ((bool)beamObject)
            {
                beamObject.transform.position = startPos + vector * 0.5f;
                ParticleSystem component2 = beamObject.GetComponent<ParticleSystem>();
                if ((bool)component2)
                {
                    ParticleSystem.ShapeModule shape = component2.shape;
                    shape.radius = totalDistance * 0.5f;
                    component2.Emit(Mathf.FloorToInt(totalDistance * beamDensity) - 1);
                }
            }
            if ((bool)startTransform)
            {
                startTransform.position = startPos;
            }
            onTailReachedDestination.AddListener(DestroySelf);
        }

        private void Update()
        {
            if (distanceTraveled > totalDistance)
            {
                onTailReachedDestination.Invoke();
                return;
            }
            distanceTraveled += speed * Time.deltaTime;
            float num = Mathf.Clamp(distanceTraveled, 0f, totalDistance);
            float num2 = Mathf.Clamp(distanceTraveled - length, 0f, totalDistance);
            if ((bool)headTransform)
            {
                headTransform.position = startPos + num * normal;
            }
            if ((bool)tailTransform)
            {
                tailTransform.position = startPos + num2 * normal;
            }
        }

        private void DestroySelf()
        {
            eventFunctions.DestroySelf();
        }
    }
}