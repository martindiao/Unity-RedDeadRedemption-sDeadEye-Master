using UnityEngine;
using DG.Tweening;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShooterController : MonoBehaviour
{
    #region Paras
    private MovementController movementController;
    [Header("Cinemachine")]
    private Camera cam;
    public CinemachineFreeLook thirdPersonCam;
    private CinemachineImpulseSource impulse;
    private float originalLens;
    private float originalYAxis;

    [Header("Aim")]
    public float zoom0;
    public float zoom1;
    public float aimDuration0;
    public float aimDuration1;
    public float rotationFactor;
    private bool isAiming = false;
    private bool isAimingAnim = false;
    private bool isAimPressed = false;
    private bool canAim = true;

    [Header("Fire")]
    public GameObject fireSparkPrefab;
    public Transform firePoint;
    public float sparkDuration;
    public AudioSource fireAudio;
    private bool isFirePressed = false;
    private bool canFire = false;

    [Header("Animation")]
    private Animator anim;
    private int isAimingHash;
    private int goFireHash;
    private int isSkillAimHash;
    private int isSkillShootHash;

    [Header("Skill")]
    public LayerMask whatIsEnemy;

    public float rotDuration;

    public float skillFireDuration;

    public float lowTimeScale;
    public float timeScaleDuration;

    public float shakeDuration;
    public float shakeStrength;

    public Color startColor;
    public Color endColor;
    public float colorDuration;

    private bool isSkillPressed;
    private bool isSkillAiming;
    private bool isSkillShooting = false;
    private bool isSkillAnim;

    private List<GameObject> targets = new List<GameObject>();

    [Header("UI")]
    public Transform canvas;
    public GameObject signUIPrefab;
    public GameObject crossUIPrefab;
    public Image maskUI;
    private List<Transform> crossUIList = new List<Transform>();

    #endregion

    #region Sys funcs
    private void Awake()
    {
        anim = GetComponent<Animator>();
        movementController = GetComponent<MovementController>();
        impulse = thirdPersonCam.GetComponent<CinemachineImpulseSource>();
    }
    private void Start()
    {
        originalLens = thirdPersonCam.m_Lens.FieldOfView;//Get the second(MIDDLE) radius
        originalYAxis = thirdPersonCam.m_YAxis.Value;

        isAimingHash = Animator.StringToHash("isAiming");
        goFireHash = Animator.StringToHash("goFire");
        isSkillAimHash = Animator.StringToHash("isSkillAim");
        isSkillShootHash = Animator.StringToHash("isSkillShoot");

        cam = Camera.main;

        Cursor.visible = false;
    }
    private void Update()
    {
        isAimPressed = Input.GetButton("Aim");
        isFirePressed = Input.GetButtonDown("Fire");
        isSkillPressed = Input.GetButton("Skill");
        HandleAim();
        HandleRotation();
        HandleFire();
        HandleSkill();
        HandleAnim();
        HandleCrossUI();

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

    }
    #endregion

    #region Aim
    private void HandleAim()
    {
        if (isAimPressed)
        {
            if (isAiming == false && canAim)
            {
                isAiming = true;
                canFire = false;
                DOVirtual.Float(originalLens, zoom0, aimDuration0, CameraZoom0);
                movementController.canMove = false;//disable movement
                signUIPrefab.SetActive(true);
            }
        }
        else
        {
            if(isAiming == true)
            {
                isAiming = false;
                canFire = true;
                DOVirtual.Float(thirdPersonCam.m_Lens.FieldOfView,originalLens, aimDuration1, CameraZoom1);
                movementController.canMove = true;
                signUIPrefab.SetActive(false);

            }
        }
    }

    private void CameraZoom0(float x)
    {
        //防止一按下就松开
        if (isAiming || isSkillAiming)
        {
            thirdPersonCam.m_Lens.FieldOfView = x;

            if ((zoom0 - x <= 2 || zoom1 - x <= 10) && canFire == false) canFire = true;//fire after aiming finishing
        }
    }
    private void CameraZoom1(float x)
    {
        thirdPersonCam.m_Lens.FieldOfView = x;
    }
    private void HandleRotation()
    {
        if(isAiming || isSkillAiming)
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(cam.transform.forward + cam.transform.right * 0.1f), rotationFactor * Time.deltaTime);
    }
    #endregion

    #region Fire
    private void HandleFire()
    {
        if (isAiming && isFirePressed && canFire)
        {
            fireAudio.Play();
            var g = Instantiate(fireSparkPrefab, firePoint.position, Quaternion.identity) as GameObject;
            Destroy(g, sparkDuration);
        }
    }
    #endregion

    #region Skill
    private void HandleSkill()
    {
        if (isSkillPressed && !isSkillAiming)
        {
            if (!isSkillAiming) isSkillAiming = true;
            movementController.canMove = false;
            canAim = false;
            DOVirtual.Float(thirdPersonCam.m_Lens.FieldOfView, zoom1, aimDuration0, CameraZoom0);
            DOVirtual.Float(1f,lowTimeScale,timeScaleDuration,SetTimeScale);

            maskUI.gameObject.SetActive(true);
            DOVirtual.Color(startColor, endColor, colorDuration,(x)=> { maskUI.color = x; });
            
            signUIPrefab.SetActive(true);
        }
        else if (!isSkillPressed && isSkillAiming && !isSkillShooting)
        {
            isSkillAiming = false;
            signUIPrefab.SetActive(false);
            if (targets.Count <= 0)
            {
                DOVirtual.Float(thirdPersonCam.m_Lens.FieldOfView, originalLens, aimDuration1, CameraZoom1);
                DOVirtual.Float(lowTimeScale, 1f, timeScaleDuration, SetTimeScale);
                maskUI.gameObject.SetActive(false);
                movementController.canMove = true;
                canAim = true;
                isSkillShooting = false;
            }

            if (targets.Count > 0)
            {
                int indexToHide = 0;
                Sequence seq = DOTween.Sequence();
                movementController.canMove = false;
                isSkillShooting = true;
                for (int i = 0; i < targets.Count; i++) 
                {
                    var target = targets[i];
                    seq.Append(transform.DOLookAt(target.transform.position, rotDuration));
                    seq.AppendInterval(rotDuration);
                    seq.AppendCallback(GoFireAnim);
                    seq.AppendInterval(skillFireDuration * (1f/3f));
                    seq.AppendCallback(() =>
                    {
                        impulse.GenerateImpulse();
                        target.GetComponentInParent<EnemyController>().Ragdoll(true, target.transform);
                        crossUIList[indexToHide].gameObject.SetActive(false);
                        indexToHide++;

                        var g = Instantiate(fireSparkPrefab, firePoint.position, Quaternion.identity) as GameObject;
                        Destroy(g, sparkDuration);

                        fireAudio.Play();

                    });
                    seq.AppendInterval(skillFireDuration * (2f/3f));
                }
                seq.AppendCallback(() => { 
                    isSkillShooting = false; 
                    movementController.canMove = true;
                    movementController.canMove = true;
                    canAim = true;
                    foreach(var g in crossUIList)
                        Destroy(g.gameObject);
                    crossUIList.Clear();
                    targets.Clear();
                    DOVirtual.Float(thirdPersonCam.m_Lens.FieldOfView, originalLens, aimDuration1, CameraZoom1);
                    DOVirtual.Float(lowTimeScale, 1f, timeScaleDuration, SetTimeScale);

                    maskUI.gameObject.SetActive(false);
                });
            }
        }
        if (isSkillAiming)
        {
            RaycastHit hit;
            Physics.Raycast(cam.transform.position,cam.transform.forward,out hit,Mathf.Infinity,whatIsEnemy);
            if(hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                var aimed = hit.transform.GetComponentInParent<EnemyController>().aimed;
                if (aimed) return;
                hit.transform.GetComponentInParent<EnemyController>().aimed = true;
                var cross = Instantiate(crossUIPrefab, canvas);
                cross.transform.position = cam.WorldToScreenPoint(hit.transform.position);
                crossUIList.Add(cross.transform);
                Debug.Log(hit.collider.gameObject.name);
                targets.Add(hit.collider.gameObject);
            }
        }
    }

    private void SetTimeScale(float x)
    {
        Time.timeScale = x;
    }


    private void HandleCrossUI()
    {
        if(crossUIList.Count > 0)
            for(int i = 0; i < crossUIList.Count; i++)
                crossUIList[i].position = cam.WorldToScreenPoint(targets[i].transform.position);
    }
    #endregion

    #region Animation
    private void HandleAnim()
    {
        isAimingAnim = anim.GetBool(isAimingHash);
        isSkillAnim = anim.GetBool(isSkillAimHash);
        if (isAimPressed && !isAimingAnim)
        {
            anim.SetBool(isAimingHash, true);
        }
        else if(!isAimPressed && isAimingAnim)
        {
            anim.SetBool(isAimingHash, false);
        }
        if (isAiming && isFirePressed && canFire)
        {
            GoFireAnim();
        }
        if(isSkillAiming && !isSkillAnim)
        {
            anim.SetBool(isSkillAimHash,true);
        }
        else if (!isSkillAiming && isSkillAnim)
        {
            anim.SetBool(isSkillAimHash, false);
        }
        anim.SetBool(isSkillShootHash, isSkillShooting);
    }
    private void GoFireAnim()
    {
        anim.SetTrigger(goFireHash);
    }
    #endregion

    private void OnDrawGizmos()
    {
        if(cam==null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine (cam.transform.position,cam.transform.position + cam.transform.forward * 1000);
    }
}
