using UnityEngine;

public class MapCameraFollow : MonoBehaviour
{
	public Transform Target;
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	public void Update ()
	{
		transform.position = new Vector3(
			Target.transform.position.x,
			Target.transform.position.y,
			transform.position.z
		);
	}
}
