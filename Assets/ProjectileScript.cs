using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    // Update is called once per frame
    [SerializeField] Transform projectileTransform;
    public Vector3 mainGridWorldDestination;
    public int projectileSpeed = 6;
    public Vector2Int movingDirection;
    private Vector2Int shooterStaringWorldPosition;
    private AnimationsEventsScript animatorEvents;

    private int distanceTraveled = 0;

    public void ShootProjectile(Vector2Int shooterPosition)
    {
        distanceTraveled = 0;
        shooterStaringWorldPosition = shooterPosition;
        animatorEvents = GetComponentInParent<AnimationsEventsScript>();
        Debug.Log("wystrzal obiektu");
        StartCoroutine(FlyingTowardDirection(movingDirection));
    }
    public List<Vector2> TargetsPositionsList = new List<Vector2>();
    public IEnumerator FlyingTowardDirection(Vector2Int direction)
    {
        Vector3 projectileGridWorldPosition = new Vector3(shooterStaringWorldPosition.x+(direction.x*distanceTraveled),shooterStaringWorldPosition.y+(direction.y*distanceTraveled),0);
       
        if(projectileGridWorldPosition == mainGridWorldDestination)
        {
            // zniszczenie obiektu strzały
            Destroy(this.gameObject);
            // pocisk dotart do celu / zanotowanie trafienia
            animatorEvents.Attack_Hit_1();
            Debug.Log("final hit");
            // wykonanie "eksplozji" TODO:
            animatorEvents.AttackAnimationFinished();
            // zakończenie animacji = nowa tura
            yield break;
        }
        else if (TargetsPositionsList.Contains(projectileGridWorldPosition))
        {
            animatorEvents.Attack_Hit_1();
            Debug.Log("pocisk przeszywajacy");
        }

        Vector3 currentProjectileobjectposition = new Vector3(projectileTransform.localPosition.x,projectileTransform.localPosition.y,0);
        Vector3 nextdestination = (currentProjectileobjectposition + new Vector3(direction.x,direction.y,0));

        float movingProgress = 0f;
        for(float i = 0f; i<=projectileSpeed; i++)
        {
            movingProgress = i/projectileSpeed;
            //Debug.Log("arrow  current position = "+ projectileTransform.localPosition + " arrow new position = "+Vector3.Lerp(projectileGridWorldPosition,nextdestination,movingProgress)+ " [ moving progress ="+movingProgress+"]");
            projectileTransform.localPosition = Vector3.Lerp(currentProjectileobjectposition,nextdestination,movingProgress);
            yield return new WaitForFixedUpdate();
        }

        distanceTraveled++;
        
        StartCoroutine(FlyingTowardDirection(direction));
        yield break;
    } 
}
