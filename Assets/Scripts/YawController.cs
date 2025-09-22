using UnityEngine;
using UnityEngine.UI;


public class YawController : MonoBehaviour{

    // Referenciar scripts
    public GetFromServer server;
    public CubeCarCameraGuide car;

    // Constantes
    private const float steering_ratio = 16f;
    private const float axis_distance = 2.7f;

    // Variaveis de entrada
    private float steering_wheel_angle = 0.0f;
    private float wheel_angle = 0.0f;
    private float speed = 0.0f;

    // Variaveis internas
    private bool initialized = false;
    private float wheel_angle_rad = 0.0f;

    // Variaveis de saida
    private float yaw_ratio = 0.0f;

    private void Update(){

        // Esperar incialização de objetos
        initialized = server.wheel != null && server.pedalAcc != null && server.pedalFreio != null;
        if(!initialized) return;

        // Pega o ângulo do volante
        steering_wheel_angle = server.wheel.raw;

        // Calcular ângulo das rodas
        wheel_angle = steering_wheel_angle / steering_ratio;
        wheel_angle_rad = wheel_angle * Mathf.Deg2Rad;

        // Pegar velocidade atual e passar de km/h para m/s
        speed = car.speed_world / 3.6f;

        // Calcular variação em graus por segundo do cenário
        yaw_ratio = (speed / axis_distance) * Mathf.Tan(wheel_angle_rad);

        // yaw par graus
        yaw_ratio *= Mathf.Rad2Deg;

        // Passar a variação de angulo atual para o CarController
        car.setYawDifference(yaw_ratio*Time.deltaTime, yaw_ratio);

    }

}
