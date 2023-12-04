using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Projection : MonoBehaviour
{

    private void Start()
    {
        CreatePhysicsScene();
    }

    private Scene _simulationScene;
    private PhysicsScene _physicsScene;

    [SerializeField] private Transform _obstaclesParent;

    void CreatePhysicsScene()
    {
        _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _simulationScene.GetPhysicsScene();

        foreach(Transform obj in _obstaclesParent)
        {
            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            if(ghostObj.GetComponent<Renderer>()!=null)
                ghostObj.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
        }
    }

    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIterations = 30;
    public void SimulateTrajectory(GameObject ballPrefab, Vector3 pos, float speed, Vector3 direction, Vector3 hitPos)
    {
        var ghostObj = Instantiate(ballPrefab, pos, Quaternion.identity);
        ghostObj.GetComponent<Renderer>().enabled = false;
        SceneManager.MoveGameObjectToScene(ghostObj.gameObject, _simulationScene);

        ghostObj.GetComponentInChildren<Rigidbody>().AddForceAtPosition(-direction * Mathf.Abs(speed) * 1000000/1.5f, hitPos);

        _line.positionCount = _maxPhysicsFrameIterations;

        for (int i=0; i<_maxPhysicsFrameIterations; i++)
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            _line.SetPosition(i, ghostObj.transform.position);
        }

        Destroy(ghostObj.gameObject);
    }
}
