using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Moveit;

public class JointsAngleChanger : MonoBehaviour
{
    const int JOINT_NUMBER = 6; 
    float[] _angles = new float[JOINT_NUMBER];

    [SerializeField] ArticulationBody[] _articulationBodies = new ArticulationBody[JOINT_NUMBER];

    [Header("Settings")]
    [SerializeField] float _stiffness = 10000f;
    [SerializeField] float _damping = 100f;
    [SerializeField] float _forceLimit = 10000f;

    void Start()
    {
        for(int jointIndex = 0; jointIndex < JOINT_NUMBER; jointIndex++)
        {
            var xDrive = _articulationBodies[jointIndex].xDrive;

            xDrive.stiffness = _stiffness;
            xDrive.damping = _damping; 
            xDrive.forceLimit = _forceLimit;

            _articulationBodies[jointIndex].xDrive = xDrive;
        }

        ROSConnector.onRobotTrajectoryMsgReceived += OnRobotTrajectoryMsgReceived;
    }

    private void OnRobotTrajectoryMsgReceived(RobotTrajectoryMsg robotTrajectoryMsg)
    {
        UnpackMessage(robotTrajectoryMsg);
        RotateJoints();
    }

    private void UnpackMessage(RobotTrajectoryMsg robotTrajectoryMsg)
    {
        int pointsNumber = robotTrajectoryMsg.joint_trajectory.points.Length;

        for(int i = 0; i < JOINT_NUMBER; i++)
        {
            // 今回は目標地点の角度のみ使用する
            _angles[i] = (float)(robotTrajectoryMsg.joint_trajectory.points[pointsNumber - 1].positions[i] * 180.0f / Math.PI);
        }
    }

    private void RotateJoints()
    {
        for(int jointIndex = 0; jointIndex < JOINT_NUMBER; jointIndex++)
        {
            ArticulationDrive articulationDrive = _articulationBodies[jointIndex].xDrive;

            articulationDrive.target = _angles[jointIndex];

            _articulationBodies[jointIndex].xDrive = articulationDrive;
        }
    }
}
