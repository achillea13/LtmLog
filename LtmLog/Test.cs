using UnityEngine;
using System.Collections;
using Ltm;

public class loca : MonoBehaviour {

	// Use this for initialization
	void Start () {
	

		LtmLog.Init(LOG_TYPE.Unity);
		LtmLog.WriteLog("test1", "1", LOG_LV.Usual, FILE_IN.Yes);
		LtmLog.WriteLog("test2");

		LtmLog.Disable("1");
		LtmLog.WriteLog("test3","1");
		LtmLog.Enable("1");
		LtmLog.WriteLog("test4");

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
