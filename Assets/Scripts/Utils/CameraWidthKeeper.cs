using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Use for keeping all the scene in the scope of camera in different resolutions of screen
/// </summary>
[ExecuteAlways]
public class CameraWidthKeeper : MonoBehaviour
{
	[SerializeField] private Camera _cam;
	[SerializeField] private CanvasScaler _canvasScaler;
	[SerializeField] private float _perfectOrthographicSize = 6.5f; // perfect size for main scene
	private float _desiredHalfWorldWidth;
	private Vector2 Reference => _canvasScaler.referenceResolution;
	private float RefAspect => Reference.x / Reference.y;

	void OnValidate()
	{
		ApplySize();
	}

	void Start()
	{
		ApplySize();
	}

	private void ApplySize()
	{
		_desiredHalfWorldWidth = _perfectOrthographicSize * RefAspect;
		float currentAspect = (float)Screen.width / Screen.height;
		_cam.orthographicSize = Mathf.Clamp(_desiredHalfWorldWidth / currentAspect, _perfectOrthographicSize, float.MaxValue);
	}
}