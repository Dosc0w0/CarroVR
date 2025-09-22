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

    // Arrasto, reduzir velocidade com o tempo
    private const float drag_coef = 0.2f;
    private bool disable_draggin = false;

    // Reduzir numericamente valor de velocidade para unity
    private const float reduce_transform_factor = 75f;

    // Flag de esperar inicializar
    private bool initialized = false;

    private void Update()
    {

        // Esperar incialização de objetos
        initialized = server.wheel != null && server.pedalAcc != null && server.pedalFreio != null;
        if(!initialized) return;

        // Aceleração efetiva recebida, pedal acc + pedal freio, -100 to 100, -1 to 1.
        acceleration = (server.pedalAcc.raw - server.pedalFreio.raw)/100;

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

        if (velocity >= max_pos_vel) {
            velocity = max_pos_vel;
        }

        else if (velocity <= max_neg_vel){
            velocity = max_neg_vel;
        }

        // -------------------- Controle de arrasto ------------------- // FEITO

        if (!disable_draggin) {

            if (velocity > 0){
                velocity = Mathf.Max(0, velocity - drag_coef);
            }

            if (velocity < 0){
                velocity = Mathf.Min(0, velocity + drag_coef);
            }
        }

        // ---------------------- Controle de re ---------------------- // TODO

        // Calcula nova velocidade instantanea

        velocity += acceleration * Time.deltaTime;
        print("Acceleration: " + acceleration + " Velocity: " + velocity);
        car.setInstantSpeed(velocity/reduce_transform_factor, velocity, acceleration);

    }
}
