using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem movementParticle;
    [SerializeField] private PacStudentController pacStudent; 

    void Update()
    {
        bool isActuallyMoving = Vector3.Distance(pacStudent.transform.position, pacStudent.targetWorldPos) > 0.001f;

        if (isActuallyMoving)
        {
            if (!movementParticle.isPlaying)
                movementParticle.Play();
        }
        else
        {
            if (movementParticle.isPlaying)
                movementParticle.Stop();
        }
    }
}
