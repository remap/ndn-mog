using UnityEngine;
using System.Collections;

//I'll just try to turn up the applicaiton configuration interface

public class ControlLabels : MonoBehaviour
{

	public enum LabelOption
	{
		none,
		self,
		players,
		stats
	};
	
	public static int currentOption;
	public static GameObject selfLabel;
	
	void Start ()
	{
		selfLabel = GameObject.Find(UnityConstants.playerTransformPath + UnityConstants.labelTransformPath);
		currentOption = (int)LabelOption.self;
		applySelf ();
	}
	
	void Update ()
	{
		if (Input.GetKeyUp (KeyCode.N)) {
			selectNextOption ();
			applyOption ();
		}
	}
	
	void selectNextOption ()
	{
		if (currentOption != (int)LabelOption.stats) {
			currentOption = currentOption + 1;
		} else {
			currentOption = (int)LabelOption.none;
		}
	}
	
	public static void applyOption ()
	{
		switch (currentOption) {
		case (int)LabelOption.none:
			applyNone ();
			break;
		case (int)LabelOption.self:
			applySelf ();
			break;
		case (int)LabelOption.players:
			applyPlayers ();
			break;
		case (int)LabelOption.stats:
			applyStats ();
			break;
		}
	}
	
	static void applyNone ()
	{
		selfLabel.SetActiveRecursively(false);
		
		
	}
	
	static void applySelf ()
	{
		selfLabel.SetActiveRecursively(true);
	}
	
	static void applyPlayers ()
	{
		selfLabel.SetActiveRecursively(true);
	}
	
	static void applyStats ()
	{
		selfLabel.SetActiveRecursively(true);
	}
}
