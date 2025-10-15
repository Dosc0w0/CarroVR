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
    private const float map_PosAcc_PosVel = 30;  // (+) Positiva
    private const float map_NegAcc_PosVel = 100;  // (-) Negativa
    private const float map_PosAcc_NegVel = 30;  // (+) Positiva
    private const float map_NegAcc_NegVel = 30;  // (-) Negativa

    // Principais variaveis
    private float acceleration = 0.0f;
    private float velocity = 0.0f;
    private int acc_efi_raw = 0;

    // Arrasto, reduzir velocidade com o tempo
    private const float drag_coef = 0.1f;

    // Reduzir numericamente valor de velocidade para unity
    private const float reduce_transform_factor = 100f;

    // Flag de esperar inicializar
    private bool initialized = false;
    private bool lockReverse = false;

    private float increment = 0.0f;
    private int raw_pedal_acc = 0;
    private int raw_pedal_brake = 0;

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

        /*
        if (velocity > 0)
        {
            acceleration = (raw_pedal_acc * map_PosAcc_PosVel - raw_pedal_brake * map_NegAcc_PosVel) / 100;
        }

        else if (velocity < 0)
        {
            acceleration = (raw_pedal_acc * map_NegAcc_NegVel - raw_pedal_brake * map_PosAcc_NegVel) / 100;
        }

        else if (velocity == 0)
        {

            if (raw_pedal_acc>0)
            {
                acceleration = (raw_pedal_acc * map_PosAcc_PosVel - raw_pedal_brake * map_NegAcc_PosVel) / 100;
            }

            else if (raw_pedal_brake>0)
            {
                acceleration = (raw_pedal_acc * map_NegAcc_NegVel - raw_pedal_brake * map_PosAcc_NegVel) / 100;
            }

        }
        */

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
