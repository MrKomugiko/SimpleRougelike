using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public int ProjectileSpeed = 6;
    public List<Vector2> TargetsPositionsList = new List<Vector2>();
    public Vector3 MainTarget_GridCoord;
    [SerializeField] private Transform projectileTransform;
    [SerializeField] private SpriteRenderer ProjectileSprite;
    public Vector2Int movingDirection;
    private Vector2Int Origin_GridCoord;
    private AnimationsEventsScript animatorEvents;
    private int distanceTraveled = 0;
    public void ShootProjectile(Vector2Int shooterPosition)
    {
        distanceTraveled = 0;
        Origin_GridCoord = shooterPosition;
        animatorEvents = GetComponentInParent<AnimationsEventsScript>();
        
        SetFlyingAngle(movingDirection);
        StartCoroutine(FlyingTowardDirection(movingDirection));
    }
    private void SetFlyingAngle(Vector2Int direction)
    {
        float angle = 0;
        if(direction == Vector2Int.up)
                angle = 180;        

        if(direction == Vector2Int.right)
                angle = 90;      

        if(direction == Vector2Int.down)
                angle = 0;       

        if(direction == Vector2Int.left)
                angle = -90;        

        ProjectileSprite.transform.Rotate(0,0,angle);
    }
    private IEnumerator FlyingTowardDirection(Vector2Int direction)
    {
        Vector3 projectileGridWorldPosition = new Vector3(Origin_GridCoord.x+(direction.x*distanceTraveled),Origin_GridCoord.y+(direction.y*distanceTraveled),0);
       
        if(projectileGridWorldPosition == MainTarget_GridCoord)
        {
            Destroy(this.gameObject);
            animatorEvents.Attack_Hit_1();
            animatorEvents.AttackAnimationFinished();
            yield break;
        }
        else if (TargetsPositionsList.Contains(projectileGridWorldPosition))
        {
            animatorEvents.Attack_Hit_1();
        }

        Vector3 currentProjectileobjectposition = new Vector3(projectileTransform.localPosition.x,projectileTransform.localPosition.y,0);
        Vector3 nextdestination = (currentProjectileobjectposition + new Vector3(direction.x,direction.y,0));

        float movingProgress = 0f;
        for(float i = 0f; i<=ProjectileSpeed; i++)
        {
            movingProgress = i/ProjectileSpeed;
            projectileTransform.localPosition = Vector3.Lerp(currentProjectileobjectposition,nextdestination,movingProgress);
            yield return new WaitForFixedUpdate();
        }

        distanceTraveled++;
        
        StartCoroutine(FlyingTowardDirection(direction));
        yield break;
    } 
}
