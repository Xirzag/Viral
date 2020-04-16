using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

using Chromosome = System.Collections.Generic.List<float>;
using CrossOver = System.Func<System.Collections.Generic.List<float>,
                             System.Collections.Generic.List<float>,
                             System.Collections.Generic.List<float>>;

public class EvolutionSystem : MonoBehaviour
{

    public int population = 20;
    public float circleDistance = 10;
    public GameObject enemyPrefab;
    public GameObject player;

    public Vector2 uniformMinMax = new Vector2(-1, 1);

    public float fitnessByHurt = 10;
    public float fitnessByLaserAvoid = 1;
    public float fitnessByDistance = 1;
    public float fitnessDistance = 1;

    // Start is called before the first frame update
    void Start()
    {
        InitSystem();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitSystem()
    {

        fitnessRanking.Clear();
        crossoverMethods.Clear();

        crossoverMethods.Add("None", CrossoverTemplates.None);
        crossoverMethods.Add("Plain", CrossoverTemplates.Plain);
        crossoverMethods.Add("Lineal", CrossoverTemplates.Lineal);
        crossoverMethods.Add("LinealMulti", CrossoverTemplates.LinealMulti);
        crossoverMethods.Add("Combinated .5", CrossoverTemplates.Combinated50);
        crossoverMethods.Add("Combinated .25", CrossoverTemplates.Combinated25);
        crossoverMethods.Add("Combinated .1", CrossoverTemplates.Combinated10);
        crossoverMethods.Add("Combinated -.1", CrossoverTemplates.CombinatedMinus10);
        crossoverMethods.Add("On a Point", CrossoverTemplates.InAPoint);
        crossoverMethods.Add("Uniform", CrossoverTemplates.Uniform);

        for (int i = 0; i < population; i++)
        {
            Chromosome chromosome = RandomUniformChromosome(uniformMinMax.x, uniformMinMax.y);
            GenerateEnemy(chromosome);
        }
    }
    private Chromosome RandomUniformChromosome(float min, float max)
    {
        Chromosome chromosome = new Chromosome();
        for (int i = 0; i < (int)EnemyControl.GEN.END; i++)
            chromosome.Add(UnityEngine.Random.Range(min, max));
        return chromosome;
    }

    public Vector3 RandomOnCircle(float radious)
    {
        float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
        return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)) * radious;
    }

    private void GenerateEnemy(Chromosome chromosome)
    {
        Vector3 position = RandomOnCircle(circleDistance);
        position += player.transform.position;  
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.GetComponent<EnemyControl>().InitChromosome(this, chromosome);
    }

    public enum DIE_REASON
    {
        GO_FAR,
        HURT_PLAYER,
        LASER
    }

    private Chromosome RandomUniformMutation(Chromosome gens, float posibility, float range)
    {
        for(int i = 0; i < gens.Count; i++)
        {
            if(UnityEngine.Random.value <= posibility)
            {
                gens[i] += UnityEngine.Random.Range(-range/2, range/2);
            }
        }
        return gens;
    }

    public string ChromosomeStr(Chromosome chromosome)
    {
        string msg = "------------\n";
        for(int i = 0; i < chromosome.Count; i++)
        {
            msg += ((EnemyControl.GEN)i).ToString() + ":" + chromosome[i] + "\n";
        }
        msg += "------------";
        return msg;
    }

    public string currentCrossoverMethod = "None";
    public float mutationProbabilty = .4f;
    public float mutationChange = .2f;

    public void EnemyDieEvent(EnemyControl enemy, DIE_REASON reason)
    {

        //print(reason.ToString() + "\n" + ChromosomeStr(enemy.GetChromosome()));

        float distance2 = Vector3.SqrMagnitude(enemy.transform.position - player.transform.position);
        float fitness = GetFitness(enemy, reason, distance2);

        AddToFitnessRanking(enemy, fitness);

        if(fitnessRanking.Count < 2)
        {
            Chromosome chromosome = RandomUniformChromosome(uniformMinMax.x, uniformMinMax.y);
            GenerateEnemy(chromosome);
        }
        else
        {
            List<FitnessData> enemies = GetRandomFromRanking(2);
            Chromosome chromosome = crossoverMethods[currentCrossoverMethod](enemies[0].chromosome, enemies[1].chromosome);
            chromosome = RandomUniformMutation(chromosome, mutationProbabilty, mutationChange);
            GenerateEnemy(chromosome);
        }

        Destroy(enemy.gameObject);

    }

    
    Dictionary<string, CrossOver> crossoverMethods = new Dictionary<string, CrossOver>();

    private List<FitnessData> GetRandomFromRanking(int n)
    {

        HashSet<int> indices = new HashSet<int>();
        List<FitnessData> selected = new List<FitnessData>();

        int rankingSize = fitnessRanking.Count;

        do
        {
            int random = UnityEngine.Random.Range(0, rankingSize);
            if(!indices.Contains(random))
            {
                selected.Add(fitnessRanking[random]);
                indices.Add(random);
            }
        } while (indices.Count < n && n < rankingSize);

        return selected;
    }

    struct FitnessData
    {
        public Chromosome chromosome;
        public int iteration;

        public FitnessData(Chromosome chromosome, int iteration)
        {
            this.chromosome = chromosome;
            this.iteration = iteration;
        }
    }

    private SortedDictionary<float, FitnessData> fitnessRanking = new SortedDictionary<float, FitnessData>();
    public float maxRankingSize = 6;
    public float minRankingFitness = 2;
    public float rankingRemoveIteration = 10;

    private int iteration = 0;

    private void AddToFitnessRanking(EnemyControl enemy, float fitness)
    {
        if (maxRankingSize == 0 || minRankingFitness < fitness) return;

        if (fitnessRanking.Count == 0)
        {
            fitnessRanking.Add(fitness, new FitnessData(enemy.GetChromosome(), iteration));
        }
        else
        {
            var minFitness = fitnessRanking.Last();
            if (minFitness.Key < fitness)
            {
                fitnessRanking.Add(fitness, new FitnessData(enemy.GetChromosome(), iteration));
                fitnessRanking.Remove(minFitness.Key);
            }
        }
        
        
    }

    public float GetFitness(EnemyControl enemy, DIE_REASON reason, float distance2)
    {
        switch (reason)
        {
            case DIE_REASON.HURT_PLAYER:
                Camera.main.GetComponent<DeathUI>().Die();
                return fitnessByHurt + enemy.detectedLasers * fitnessByLaserAvoid;
            case DIE_REASON.LASER:
                return Mathf.Max(0, fitnessDistance - distance2) * fitnessByDistance
                    + enemy.detectedLasers * fitnessByLaserAvoid;
            default:
                Debug.Log("DIEEEE");
                return 0;
        }

    }

}
