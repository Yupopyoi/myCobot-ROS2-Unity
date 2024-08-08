using System;
using UnityEngine;
using RosMessageTypes.Sensor;

public class JointStatePublisher : MonoBehaviour
{
    const int JOINT_NUMBER = 6;

    [SerializeField] ArticulationBody[] _articulationBodies = new ArticulationBody[JOINT_NUMBER];

    string[] _jointName = new string[6]{
        "joint2_to_joint1", "joint3_to_joint2" , "joint4_to_joint3" , "joint5_to_joint4" , "joint6_to_joint5" , "joint6output_to_joint6"
    };

    public static event Action<JointStateMsg> OnJointStateUpdated;
    
    [SerializeField,Range(0.02f,2.0f)] float _publishMessageFrequency = 0.5f;
    private float _timeElapsed;

    void Update()
    {
        _timeElapsed += Time.deltaTime;
        
        if (_timeElapsed < _publishMessageFrequency) return;

	
        var _jointStateMessage = new JointStateMsg
        {
            name = new string[_articulationBodies.Length],
            position = new double[_articulationBodies.Length],
            velocity = new double[_articulationBodies.Length],
            effort = new double[_articulationBodies.Length],
            header = new RosMessageTypes.Std.HeaderMsg
            {
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    nanosec = (uint)(DateTime.Now.Millisecond * 1000000),
                },
                frame_id = ""
            },
        };

        for (int i = 0; i < _articulationBodies.Length; i++)
        {
            _jointStateMessage.name[i] = _jointName[i];
            _jointStateMessage.position[i] = (_articulationBodies[i].jointPosition)[0];
        }

        OnJointStateUpdated?.Invoke(_jointStateMessage);
        _timeElapsed = 0;
    }
}
