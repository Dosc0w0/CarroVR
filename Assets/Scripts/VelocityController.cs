using UnityEngine;
using UnityEngine.UI;

public class VelocityController : MonoBehaviour
{
    // Referencias dos outros scripts
    public GetFromServer server;
    public CubeCarCameraGuide car;

    // Velocidades maximas
    private const float max_pos_vel = 300f;
    private const float max_neg_vel = -50f;

    // 4 estados de aceleração, vel+ e vel-, acc+ e acc-
    private const float map_PosAcc_PosVel = 20;  // (+) Positiva
    private const float map_NegAcc_PosVel = 100;  // (-) Negativa
    private const float map_PosAcc_NegVel = 20;  // (+) Positiva
    private const float map_NegAcc_NegVel = 20;  // (-) Negativa

    // Principais variaveis
    [SerializeField] private float acceleration = 0.0f;
    public float Acceleration => acceleration;

    [SerializeField] private float velocity = 0.0f;
    public float Velocity => velocity;

    [SerializeField] private int acc_efi_raw = 0;
    public int AccEfiRaw => acc_efi_raw;

    // ==========================================
    // NOVAS VARIÁVEIS DE TELEMETRIA
    // ==========================================
    [Header("Telemetria")]
    [Tooltip("Zona morta do freio (0 a 100). Ignora peso do pé e trepidação.")]
    [SerializeField] private int brakeDeadzone = 5;
    private bool wasBraking = false;

    [SerializeField] private int raw_pedal_acc = 0;
    public int RawPedalAcc => raw_pedal_acc;

    [SerializeField] private int raw_pedal_brake = 0;
    public int RawPedalBrake => raw_pedal_brake;
    // ==========================================

    // Arrasto, reduzir velocidade com o tempo
    private const float drag_coef = 0.1f;

    // Reduzir numericamente valor de velocidade para unity
    private const float reduce_transform_factor = 100f;

    // Flag de esperar inicializar
    private bool initialized = false;
    private bool lockReverse = false;

    private float increment = 0.0f;

    private void Update()
    {

        // Esperar incialização de objetos
        initialized = server.wheel != null && server.pedalAcc != null && server.pedalFreio != null;
        if(!initialized) return;

        // Aceleração efetiva recebida, pedal acc + pedal freio, -100 to 100, -1 to 1.
        raw_pedal_acc = (int)server.pedalAcc.raw;
        raw_pedal_brake = (int)server.pedalFreio.raw;
        acc_efi_raw = raw_pedal_acc - raw_pedal_brake;
        acceleration = ((float)acc_efi_raw) / 100;

        // ==========================================
        // GATILHO DE TELEMETRIA: FREIO (Para o JSON)
        // ==========================================
        bool isBrakingNow = raw_pedal_brake > brakeDeadzone; 
        
        if (isBrakingNow && !wasBraking)
        {
            // O evento dispara apenas 1 vez quando o usuário "pisa"
            SimulationEvents.TriggerBrakeApplied(raw_pedal_brake / 100f);
        }
        
        wasBraking = isBrakingNow; // Atualiza o estado para o próximo frame
        // ==========================================

        // ------------------ Controle de aceleração ------------------ // FEITO

        // Aceleração positiva

        if (acceleration > 0){

            // Se a velocidade for maior ou igual a 0
            if (velocity >= 0){
                acceleration *= map_PosAcc_PosVel;
            }

            // Se a velocidade for menor que 0
            else{
                acceleration *= map_PosAcc_NegVel;
            }
        }

        // Aceleração negativa
        else if (acceleration < 0){

            // Se a velocidade for menor ou igual a 0
            if (velocity <= 0){
                acceleration *= map_NegAcc_NegVel;
            }

            // Se a velocidade for maior que 0
            else{
                acceleration *= map_NegAcc_PosVel;
            }
        }

        // ------------------ Controle de velocidade ------------------ // TODO

        // Calular incremento para saber velocidade futura estimada
        increment = acceleration * Time.deltaTime;

        if (velocity + increment >= max_pos_vel) {
            velocity = max_pos_vel-increment;
        }

        else if (velocity + increment <= max_neg_vel){
            velocity = max_neg_vel-increment;
        }

        // -------------------- Controle de arrasto ------------------- // FEITO

        if (velocity > 0 && acc_efi_raw <= 0)
        {
            velocity = Mathf.Max(0, velocity - drag_coef);
        }

        else if (velocity < 0 && acc_efi_raw >= 0)
        {
            velocity = Mathf.Min(0, velocity + drag_coef);
        }

        // ---------------------- Controle de re ---------------------- // FEITO

        if (velocity > 0)
        {
            if (acc_efi_raw < 0)
            {
                lockReverse = true;
            }
            else
            {
                lockReverse = false;
            }
        }
        else if ((velocity + increment) <= 0)
        {
            if (acc_efi_raw < 0 && lockReverse)
            {
                velocity = 0;
                acceleration = 0;
            }
            else {
                lockReverse = false;
            }
        }

        // Recalular incremento para saber velocidade futura real
        increment = acceleration * Time.deltaTime;
        velocity += increment;
        print("LockReverse: " + lockReverse + ", Vel inc: " + increment);
        //print("Acceleration: " + acceleration + " Velocity: " + velocity);
        car.setInstantSpeed(velocity/reduce_transform_factor, velocity, acceleration);

    }
}
