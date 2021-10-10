using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AmmunitionItem;

public class ProjectileScript : MonoBehaviour
{
    public int ProjectileSpeed = 4;
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
        {
            ProjectileSprite.transform.localPosition = new Vector3(0f,1f,0);
            angle = 180;        
        }

        if(direction == Vector2Int.right)
        {          
            ProjectileSprite.transform.localPosition = new Vector3(.732f,.5f,0);
            angle = 90;      
        }

        if(direction == Vector2Int.down)
        {
            ProjectileSprite.transform.localPosition = new Vector3(0,0,0);
            angle = 0;       
        }

        if(direction == Vector2Int.left)
        {
            ProjectileSprite.transform.localPosition = new Vector3(-.5f,.5f,0);
            angle = -90;        
        }

        ProjectileSprite.transform.Rotate(0,0,angle);
    }
    private IEnumerator FlyingTowardDirection(Vector2Int direction)
    {
        Vector3 projectileGridWorldPosition = new Vector3(Origin_GridCoord.x+(direction.x*distanceTraveled),Origin_GridCoord.y+(direction.y*distanceTraveled),0);
       
        if(projectileGridWorldPosition == MainTarget_GridCoord)
        {
            Destroy(this.gameObject);
            animatorEvents.Attack_Hit_1();

            animatorEvents.AllProjectilesReachTarget = true;
            Debug.Log("wszytskie pociski dotarly do celu");
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
            yield return new WaitForEndOfFrame();
        }

        distanceTraveled++;
        
        StartCoroutine(FlyingTowardDirection(direction));
        yield break;
    } 

    public void LoadAmmoByType( SkillBase skill, AmmunitionType ammoType)
    {
        var availableAmmunition = SkillsManager.CurrentAvailableAmmo;
        
        Debug.Log("Wybranie amunicji, - aktualnie pierwsze dostępne ammo");
        AmmunitionItem selectedAmmo = availableAmmunition.First(ammo=>ammo.Key._Type == ammoType).Key;

        // ściągniecie ze stanu konkretnego itemka 
        PlayerManager.instance._mainBackpack.TakeItemFromBackpack(_takeCount:1, _findingItem: selectedAmmo);
        
        // change skill damage based on ammunition damage
        skill.CurrentDamageMultiplifer = skill.BaseDamageMultiplifer + selectedAmmo.BaseDamageMultiplifer;
    }
}
