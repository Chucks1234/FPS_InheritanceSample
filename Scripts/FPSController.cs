using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;

public class FPSController : MonoBehaviour
{
    // references
    CharacterController controller;
    [SerializeField] GameObject cam;
    [SerializeField] Transform gunHold;
    [SerializeField] Gun initialGun;

    // stats
    [SerializeField] float movementSpeed = 2.0f;
    [SerializeField] float lookSensitivityX = 1.0f;
    [SerializeField] float lookSensitivityY = 1.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpForce = 10;

    // private variables
    Vector3 origin;
    Vector3 velocity;
    bool grounded;
    float xRotation;
    List<Gun> equippedGuns = new List<Gun>();
    int gunIndex = 0;
    Gun currentGun = null;
    Vector2 like;
    Vector2 looking;
    float health = 10;

    // properties
    public GameObject Cam { get { return cam; } }
    

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // start with a gun
        if(initialGun != null)
            AddGun(initialGun);

        origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Look();
        //HandleSwitchGun();
        //FireGun();

        // always go back to "no velocity"
        // "velocity" is for movement speed that we gain in addition to our movement (falling, knockback, etc.)
        Vector3 noVelocity = new Vector3(0, velocity.y, 0);
        velocity = Vector3.Lerp(velocity, noVelocity, 5 * Time.deltaTime);
    }

    void Movement()
    {
        grounded = controller.isGrounded;

        if(grounded && velocity.y < 0)
        {
            velocity.y = -1;// -0.5f;
        }

        //Vector2 movement = GetPlayerMovementVector();
        Vector3 move = transform.right * like.x + transform.forward * like.y;
        controller.Move(move * movementSpeed * (GetSprint() ? 2 : 1) * Time.deltaTime);

        //if (Input.GetButtonDown("Jump") && grounded)
        //{
        //    velocity.y += Mathf.Sqrt (jumpForce * -1 * gravity);
        //}

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        
        float lookX = looking.x * lookSensitivityX * Time.deltaTime;
        float lookY = looking.y * lookSensitivityY * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * lookX);
    }

    void HandleSwitchGun()
    {
        //if (equippedGuns.Count == 0)
        //    return;

        //if(Input.GetAxis("Mouse ScrollWheel") > 0)
        //{
        //    gunIndex++;
        //    if (gunIndex > equippedGuns.Count - 1)
        //        gunIndex = 0;

        //    EquipGun(equippedGuns[gunIndex]);
        //}

        //else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        //{
        //    gunIndex--;
        //    if (gunIndex < 0)
        //        gunIndex = equippedGuns.Count - 1;

        //    EquipGun(equippedGuns[gunIndex]);
        //}
    }

    //void FireGun()
    //{
    //    // don't fire if we don't have a gun
    //    if (currentGun == null)
    //        return;

    //    // pressed the fire button
    //    if(GetPressFire())
    //    {
    //        currentGun?.AttemptFire();
    //    }

    //    // holding the fire button (for automatic)
    //    else if(GetHoldFire())
    //    {
    //        if (currentGun.AttemptAutomaticFire())
    //            currentGun?.AttemptFire();
    //    }

    //    // pressed the alt fire button
    //    if (GetPressAltFire())
    //    {
    //        currentGun?.AttemptAltFire();
    //    }
    //}

    void EquipGun(Gun g)
    {
        // disable current gun, if there is one
        currentGun?.Unequip();
        currentGun?.gameObject.SetActive(false);

        // enable the new gun
        g.gameObject.SetActive(true);
        g.transform.parent = gunHold;
        g.transform.localPosition = Vector3.zero;
        currentGun = g;

        g.Equip(this);
    }

    // public methods

    public void AddGun(Gun g)
    {
        // add new gun to the list
        equippedGuns.Add(g);

        // our index is the last one/new one
        gunIndex = equippedGuns.Count - 1;

        // put gun in the right place
        EquipGun(g);
    }

    public void IncreaseAmmo(int amount)
    {
        currentGun.AddAmmo(amount);
    }

    public void Respawn()
    {
        transform.position = origin;
    }

    // Input methods

    bool GetPressFire()
    {
        return Input.GetButtonDown("Fire1");
    }

    bool GetHoldFire()
    {
        return Input.GetButton("Fire1");
    }

    bool GetPressAltFire()
    {
        return Input.GetButtonDown("Fire2");
    }

    Vector2 GetPlayerMovementVector()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    Vector2 GetPlayerLook()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    bool GetSprint()
    {
        return Input.GetButton("Sprint");   
    }

    public void OnJump()
    {
        if (grounded)
        {
            velocity.y += Mathf.Sqrt(jumpForce * -1 * gravity);
        }
    }

    public void OnMovement(InputValue whatevername)
    {
        like = whatevername.Get<Vector2>();
    }

    public void OnFire()
    {
        // don't fire if we don't have a gun
        if (currentGun == null)
            return;

        // pressed the fire button
        if (GetPressFire())
        {
            currentGun?.AttemptFire();
        }

        // holding the fire button (for automatic)
        else if (GetHoldFire())
        {
            if (currentGun.AttemptAutomaticFire())
                currentGun?.AttemptFire();
        }

        // pressed the alt fire button
        if (GetPressAltFire())
        {
            currentGun?.AttemptAltFire();
        }
    }

    public void OnLook(InputValue looker)
    {
        looking = looker.Get<Vector2>();
    }

    public void OnScroll(InputValue Brolyve)
    {
        float storage = Brolyve.Get<float>();
        if (equippedGuns.Count == 0)
            return;

        if (storage > 0)
        {
            gunIndex++;
            if (gunIndex > equippedGuns.Count - 1)
                gunIndex = 0;

            EquipGun(equippedGuns[gunIndex]);
        }

        else if (storage < 0)
        {
            gunIndex--;
            if (gunIndex < 0)
                gunIndex = equippedGuns.Count - 1;

            EquipGun(equippedGuns[gunIndex]);
        }
    }

    public void OnPause()
    {
        if (!SceneManager.GetSceneByName("PauseMenu").isLoaded)
        {
            SceneManager.LoadScene("PauseMenu", LoadSceneMode.Additive);
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
        }
        else { SceneManager.UnloadSceneAsync("PauseMenu"); Time.timeScale = 1; Cursor.lockState = CursorLockMode.Locked; }

    }

    public void SavePlayerData()
    {
        FPSSaveData saveData = new FPSSaveData();
        saveData.position = this.transform.position;
        saveData.ammo = this.currentGun.GetAmmo();
        saveData.health = this.health;
        string saveText = JsonUtility.ToJson(saveData);
        Debug.Log(saveText);
        File.WriteAllText(Application.dataPath + "/saveData.json", saveText);
    }

    public void LoadPlayerData()
    {
        if (!File.Exists(Application.dataPath + "/saveData.json"))
        {
            Debug.Log("File does not exist.");
            return;
        }
        string saveText = File.ReadAllText(Application.dataPath + "/saveData.json");
        FPSSaveData loadData = JsonUtility.FromJson<FPSSaveData>(saveText);
        controller.enabled = false;
        this.transform.position = loadData.position;
        this.currentGun.AddAmmo(loadData.ammo);
        this.health = loadData.health;
        controller.enabled = true;
        Debug.Log(loadData);
        SceneManager.UnloadSceneAsync("PauseMenu"); Time.timeScale = 1; Cursor.lockState = CursorLockMode.Locked;
    }

    // Collision methods

    // Character Controller can't use OnCollisionEnter :D thanks Unity
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.GetComponent<Damager>())
        {
            var collisionPoint = hit.collider.ClosestPoint(transform.position);
            var knockbackAngle = (transform.position - collisionPoint).normalized;
            velocity = (20 * knockbackAngle);
        }

        if (hit.gameObject.GetComponent <KillZone>())
        {
            Respawn();
        }
    }


}
    public class FPSSaveData
    {
        public Vector3 position;
        public int ammo;
        public float health;
    }
