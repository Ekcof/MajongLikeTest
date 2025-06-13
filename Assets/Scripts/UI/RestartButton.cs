using Majong.Level;
using Majong.Tiles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class RestartButton : MonoBehaviour
{
    [Inject] private ILevelManager levelManager;
    private Button _button;

	void Start()
    {
		_button = GetComponent<Button>();   
		_button.SetListener(levelManager.Restart);
    }
}
