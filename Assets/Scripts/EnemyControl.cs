using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public enum GEN{
        MASS,
        SIZE,

        SPEED_TO_PLAYER,
        SPEED_DISTANCE_REL,
        SPEED_DISTANCE_2_REL,
        SPEED_DISTANCE_3_REL,
        SPEED_TOTAL,
        
        LATERAL_FREQ,
        LATERAL_FREQ_REL,
        LATERAL_AMPLITUDE,
        LATERAL_AMPLITUDE_REL,

        RED,
        GREEN,
        BLUE,

        DASH,
        DASH_REDUCE_TIME,

        DETECT_RADIOUS,
        DETECT_RADIOUS_REL,

        SPEED_WHEN_NOT_SEEING,

        END
    }

    public float maxDistanceToPlayer = 50;

    private List<float> chromosome;

    private Rigidbody2D rb;
    private SpriteRenderer renderer;
    private CircleCollider2D collider;
    private CircleCollider2D detector;

    private EvolutionSystem evolutionSystem;

    private bool died = false;

    private float dashImpulse = -Mathf.Infinity;

    public int detectedLasers = 0;
    public GameObject deathFx;
    public float deathColor = 0.2f;
    public Vector2 explosionPitch = new Vector2(.75f, .25f);
    public float explosionDecay = .2f;
    public float explosionInit = .8f;

    private float detectRadious;

    void Start()
    {

    }

    public List<float> GetChromosome()
    {
        return chromosome;
    }

    public void InitChromosome(EvolutionSystem evolutionSystem, List<float> chromosome)
    {
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        detector = GetComponent<CircleCollider2D>();
        collider = transform.GetChild(0).GetComponent<CircleCollider2D>();

        this.evolutionSystem = evolutionSystem;
        this.chromosome = chromosome;

        rb.mass = GetUnitGen(GEN.MASS);
        renderer.color = new Color(GetUnitGen(GEN.RED), GetUnitGen(GEN.GREEN), GetUnitGen(GEN.BLUE));

        transform.localScale *= GetUnitGen(GEN.SIZE) + .5f;
        collider.radius *= GetUnitGen(GEN.SIZE) + .5f;

        detector.radius *= GetUnitGen(GEN.DETECT_RADIOUS);
        detectRadious = detector.radius;

    }

    private float GetGen(GEN gen)
    {
        return chromosome[(int)gen];
    }

    private float GetUnitGen(GEN gen)
    {
        return (GetGen(gen) + 1) / 2;
    }

    // Update is called once per frame
    void FixedUpdate()
    {


        Vector3 vectorToPlayer = transform.position - evolutionSystem.player.transform.position;
        float distanceToPlayer = vectorToPlayer.magnitude;

        if (distanceToPlayer > maxDistanceToPlayer) Die(EvolutionSystem.DIE_REASON.GO_FAR);

        float relDist = RelDist(distanceToPlayer);

        detector.radius = relDist * GetGen(GEN.DETECT_RADIOUS_REL) + detectRadious;

        Vector3 force = Vector3.zero;
        force += vectorToPlayer.normalized * GetGen(GEN.SPEED_TO_PLAYER);
        force += vectorToPlayer.normalized * relDist * GetGen(GEN.SPEED_DISTANCE_REL);
        force += vectorToPlayer.normalized * relDist * relDist * GetGen(GEN.SPEED_DISTANCE_2_REL);
        force += vectorToPlayer.normalized * relDist * relDist *
            relDist * GetGen(GEN.SPEED_DISTANCE_2_REL);

        force.Normalize();
        force *= GetUnitGen(GEN.SPEED_TOTAL);

        Vector3 playerForward = evolutionSystem.player.transform.up;
        force += vectorToPlayer.normalized * Vector3.Angle(playerForward, vectorToPlayer.normalized) / 180 *
            GetUnitGen(GEN.SPEED_WHEN_NOT_SEEING);

        Vector3 horizontalMove = Vector3.Cross(vectorToPlayer, Vector3.forward);

        float freq = (GetGen(GEN.LATERAL_FREQ) + GetGen(GEN.LATERAL_AMPLITUDE_REL) * relDist);
        float amplitude = (GetGen(GEN.LATERAL_AMPLITUDE) + GetGen(GEN.LATERAL_AMPLITUDE_REL)* relDist);

        force += horizontalMove * Mathf.Sin(Time.deltaTime * freq) * amplitude * .3f;

        if (dashImpulse > -1)
        {
            force += horizontalMove * dashImpulse * GetGen(GEN.DASH) * 1.2f;
            dashImpulse -= Time.deltaTime * GetUnitGen(GEN.DASH_REDUCE_TIME);
        }

        rb.AddForce(force);

    }

    public float RelDist(float distance)
    {
        return distance / evolutionSystem.circleDistance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Laser"))
        {
            dashImpulse = 1;
            detectedLasers++;
        }

    }   

    private void OnDetection(Collider2D other, float distance)
    {
        if (other.CompareTag("Laser"))
            dashImpulse = 1;            
    }

    public void Die(EvolutionSystem.DIE_REASON reason)
    {
        if (died) return;

        GameObject fx = Instantiate(deathFx, transform.position, Quaternion.identity);
        var main = fx.GetComponent<ParticleSystem>().main;
        main.startColor = new Color(GetUnitGen(GEN.RED), GetUnitGen(GEN.GREEN), GetUnitGen(GEN.BLUE));
        AudioSource audio = fx.GetComponent<AudioSource>();
        audio.pitch = Random.Range(explosionPitch.x, explosionPitch.y);
        Vector3 cameraPos = Camera.main.transform.position;
        cameraPos.z = transform.position.z;
        float distance = Vector3.Distance(cameraPos, transform.position);
        audio.volume = explosionInit - distance * explosionDecay;

        Destroy(fx, 2.2f);

        died = true;
        evolutionSystem.EnemyDieEvent(this, reason);
    }
}
