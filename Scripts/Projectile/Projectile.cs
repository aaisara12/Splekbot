using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 speed;
    public Rigidbody backbounds;
    public Rigidbody leftbounds;
    public Rigidbody rightbounds;
    public Rigidbody Ebackbounds;
    public Rigidbody Eleftbounds;
    public Rigidbody Erightbounds;

    //private Vector3 lastVelocity;
    private ImpactPoint lastImpact; 

    private float z_back;
    private float z_net;

    private float x_right;
    private float x_left;
    private float x_mid;

    private float z_eback;
    private float x_eright;
    private float x_eleft;

    private float x_final;
    private float z_final;


    private float blockConstant = 0.5f;
    [SerializeField] private float gravity = 10f;
    private float energyLoss = 0.8f;
    private float flightTime;
    private float flightTime2;

    private ImpactPoint secondBounce;

    private AIInput aiInput;

    public int counter;
    public float hitStrength;

    [Range(0.0f, 10.0f)]
    public float boostConstant = 1.0f;


    //Match Manager Things (Added by Aidan)
    public int bounceCount;

    public event System.Action<Vector3> OnCalculateLocation;

    private AudioManager audioManager;
    private matchController match;

    // Start is called before the first frame update


    // 
    private int lastBounceSide=1;
    CourtController court;
    

    //
    [SerializeField] private string playerObjectName;
    private string aiObjectName;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        match=FindObjectOfType<matchController>();

        court = FindObjectOfType<CourtController>();

        // z_back = backbounds.position.z;
        // z_net = court.getMidpoint().z;
        // x_right = rightbounds.position.x;
        // x_left = leftbounds.position.x;

        // z_eback = Ebackbounds.position.z;
        // x_eright = Erightbounds.position.x;
        // x_eleft = Eleftbounds.position.x;

        

        z_net = court.getMidpoint().z;

        z_back = court.getTopCourtZ();
        x_right = court.getRightCourtX();
        x_left = court.getLeftCourtX();

        z_eback = court.getBotCourtZ();
        x_eright = court.getRightCourtX();
        x_eleft = court.getLeftCourtX();
    }
    void Start()
    {
        bounceCount=0;
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager Found!!!!");
        }

        secondBounce = new ImpactPoint(Vector3.zero,0);
        //speed = new Vector3(4, 3, 0);
        rb.velocity = speed;
        //string z_backs = z_back.ToString();
        //Debug.Log("z back = " + z_backs);
        hitStrength = 1.0f;

        aiInput = FindObjectOfType<AIInput>();
        playerObjectName = match.playerName();
        aiObjectName = match.AIName();
        setVelocity(Vector3.zero);


            
    }

    void Update()
    {
        //lastVelocity = rb.velocity;

        // See if we've left the bounds
        if(lastHitter != null && !court.positionInsideSidelines(transform.position,1)){

            Debug.Log("Last hitter name: " + lastHitter.gameObject.name);
            Debug.Log("Player object name: " +playerObjectName);
            Debug.Log("Position:"+transform.position);


            if(lastHitter.gameObject.name==playerObjectName){
                OnProjectileScored?.Invoke(aiObjectName);
                lastHitter=null;
                Debug.Log("Player hit out");
                
            }
            else if(lastHitter.gameObject.name==aiObjectName){
                OnProjectileScored?.Invoke(playerObjectName);
                lastHitter=null;
            }
            setVelocity(Vector3.zero);

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 myVector;

        if (rb.position.y < 0.1f && rb.position.y >-0.1f)
        {

            counter++;
            //Debug.Log("hitting ground" + counter);
            if (counter == 1)
            {
                myVector = new Vector3(rb.velocity.x, Mathf.Abs(rb.velocity.y) * energyLoss , rb.velocity.z);
                bounceCount++;
            }
            else
            {
                myVector = new Vector3(rb.velocity.x, Mathf.Abs(rb.velocity.y), rb.velocity.z);
            }
        }
        else
        {
            counter = 0;
            myVector = new Vector3(rb.velocity.x, rb.velocity.y - gravity * Time.deltaTime, rb.velocity.z);
        }
        rb.velocity = myVector;
    }

    public void setVelocity(Vector3 v){
        rb.velocity=v;
    }
    
    // TODO: velocity cap, ball state (player vs enemy ball), different boundaries based on state
    public CharacterSwing lastHitter
    {
        get;
        set;
    }



    public void Hit(Vector3 velocity, CharacterSwing hitter)
    {


        Vector3 comparison = new Vector3(1.0f, 1.0f, 1.0f);
        if (velocity == comparison)
        {
            Debug.Log("Enemy hit triggered");
            z_final = UnityEngine.Random.Range(z_eback, z_net);
            x_final = UnityEngine.Random.Range(x_eright, x_eleft);

            Vector3 hitLocation = new Vector3(x_final, 0.0f, z_final);

            Vector3 hitDir = hitLocation - transform.position;
            hitDir.Normalize();
            hitDir *= hitStrength;
            Hit(hitDir, hitter);
        }

        else
        {
            Vector3 addHeight;

            //z_final = Random.Range(z_back, z_net);
            z_final = z_net + (z_back - z_net)*Mathf.Abs(velocity.z)/10;

            // audioManager.PlaySoundAtLocation("racket_hit", transform.position);

            //ebug.Log("Hit with velocity: " + velocity);
            //velocity.y = 10;
            rb.velocity = velocity.normalized * (velocity.magnitude + rb.velocity.magnitude);
            if (rb.velocity.magnitude > 10)
                rb.velocity = rb.velocity.normalized * 10;

            if(rb.velocity.magnitude > 9)
            {
                audioManager.PlaySoundAtLocation("firework_2", transform.position);
            }
            else
            {
                audioManager.PlaySoundAtLocation("racket_hit", transform.position);
            }
            flightTime = (z_final - rb.position.z) / rb.velocity.z;
            x_final = rb.position.x + rb.velocity.x * flightTime;

            addHeight = new Vector3(rb.velocity.x, gravity * flightTime / (2), rb.velocity.z);
            rb.velocity = addHeight;

            lastHitter = hitter;


            float z_final2 = z_final + (z_final - rb.position.z) * energyLoss;
            float x_final2 = x_final + (x_final - rb.position.x) * energyLoss;
            flightTime2 = flightTime * (1 + energyLoss);

            secondBounce.impactLocation = new Vector3(x_final2, 0.0f, z_final2);
            secondBounce.impactTime = Time.time + flightTime2;

            lastImpact = secondBounce;
            //aiInput.setImpactPoint(secondBounce);
            //Debug.Log(secondBounce.impactLocation.x);
            
            if (secondBounce.impactLocation.x > court.getRightCourtX() || secondBounce.impactLocation.x < court.getLeftCourtX())//todo calibrate with WALL.transform.position.x
            {
                // Debug.Log("going in");

                float xdiff;

                xdiff = 2 * (lastImpact.impactLocation.x - court.getRightCourtX());//todo
                if (xdiff < 0)
                {
                    xdiff = 2 * (lastImpact.impactLocation.x - court.getLeftCourtX());
                }
                secondBounce.impactLocation.x = lastImpact.impactLocation.x - xdiff;

                // Debug.Log(secondBounce.impactLocation);


            }
            //added statement to support tutorial mode
            if(aiInput!=null)
                aiInput.setImpactPoint(secondBounce);
        }


        //reset our bounce count 
        bounceCount=0;
    }

    public void Block(CharacterSwing hitter)
    {
        // In the real thing, this should probably just do a weak hit in the direction of the opponent
        Debug.Log("Blocked");

        audioManager.PlaySoundAtLocation("racket_hit", transform.position);

        Vector3 hitterDirection;
        if(lastHitter != null)
            hitterDirection = (lastHitter.transform.position - transform.position).normalized;
        else
            hitterDirection = hitter.transform.forward;
        rb.velocity = hitterDirection * (rb.velocity.magnitude) * blockConstant;
        lastHitter = hitter;
    }


    //bouncing off wall
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))//EDIT THIS
        {
            // Debug.Log("hit the wall yikes");
            //var speed = lastVelocity.magnitude;

            //var contact = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            //var direction = Vector3.Reflect(lastVelocity.normalized, contact.normal);
            //rb.velocity = direction * Mathf.Max(speed, 0f); 
            Vector3 myVector;
            // float xdiff;

            myVector = new Vector3(-1*rb.velocity.x, rb.velocity.y, rb.velocity.z);
            rb.velocity = myVector;
            // Debug.Log(lastImpact.impactLocation.x);
            // Debug.Log(rb.transform.position.x);
            //xdiff = 2 * (lastImpact.impactLocation.x - rb.transform.position.x);
            //Debug.Log(lastImpact);
           // Debug.Log(xdiff);
            //lastImpact.impactLocation.x = lastImpact.impactLocation.x - xdiff;

            // Debug.Log("trigger" + lastImpact.impactLocation.x);
            //aiInput.setImpactPoint(lastImpact);
        }


        // Aaron: Add this to explode projectile
        if(other.gameObject.CompareTag("Goal"))
        {

            ExplodeProjectile();
            // audioManager.PlaySoundAtLocation("explosion", transform.position);
            audioManager.PlaySound("explosion");
        }

    }


    public int getBounceCount(){
        return bounceCount;
    }
    public void resetBounceCount(){
        bounceCount=0;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.GetMask("Ground")){
            //if the side we bounced on was different than our last bounce side, swap it to this new side
            int currentSide = court.getSide(transform.position);
            if(currentSide==lastBounceSide)
                bounceCount++;
            else{
                lastBounceSide=currentSide;
                bounceCount=1;
            }

        }


    }

    [SerializeField] ParticleSystem explosionParticles;
    public event System.Action<string> OnProjectileScored;        // Invoked with CharacterSwing component of last person to hit ball
    void ExplodeProjectile()
    {
        GetComponent<SpriteRenderer>().enabled = false;        // Make the projectile "disappear"
        GetComponent<TrailRenderer>().enabled = false;
        
        // TODO: Disable shadow for projectile (shadow script currently does not have a function to disable shadows)

        if(explosionParticles != null)
            GameObject.Instantiate(explosionParticles, transform.position, Quaternion.identity);
        
        OnProjectileScored?.Invoke(lastHitter.gameObject.name);
        setVelocity(Vector3.zero);
    }

    public void hideProjectile(){
        GetComponent<SpriteRenderer>().enabled = false;      
        GetComponent<TrailRenderer>().enabled = false;
    }
    public void showProjectile(){
        GetComponent<SpriteRenderer>().enabled = true;      
        TrailRenderer t = GetComponent<TrailRenderer>();
        t.enabled=true;
        Vector3[] positions = {transform.position};
        t.SetPositions(positions);
    }

}