﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FunkyManager : MonoBehaviour {
	public float funkMeter;
	GameObject[] players;

	public AnimationCurve decreaseCurve;

	public float maxDistance = 30.0f;
	public float maxFunk = 900.0f;
	public float activityThreshold = 15f;
	public Image fillerBar;
	public Image[] activityIndicators;
	public Color activityColor = Color.blue;
	public Image endOfGame;
	public Text timerText;
	public Text finalText;
	private float levelStart;

	void Start () {
		players = GameObject.FindGameObjectsWithTag ("Player");
		levelStart = Time.time;
	}
		
	void Update () {
		if (endOfGame.gameObject.activeSelf) {
			if (Input.GetButton ("Space")) {
				SceneManager.LoadScene ("Game");
			}
		} else {
			int seconds = Mathf.FloorToInt(Time.time - levelStart);
			timerText.text = string.Format ("{0:00}:{1:00}:{2:00}", seconds / 3600, (seconds % 3600) / 60, seconds % 60);
			funkMeter -= Mathf.Max (Time.deltaTime * 1.9f / 3 * decreaseCurve.Evaluate (funkMeter), 0);
			funkMeter = Mathf.Max (funkMeter, 0);

			List<GameObject> funkyPlayers = new List<GameObject> ();
			int totalFunky = 0;
			List<bool> activePlayers = new List<bool> ();
			int totalActive = 0;

			foreach (GameObject go in players) {
				FunkyControl funkyControl = go.GetComponent<FunkyControl> ();
				if (funkyControl.isFunky) {
					funkyPlayers.Add (funkyControl.gameObject);
					++totalFunky;
				} else {
					funkyPlayers.Add (null);
				}
				LastActivity lastActivity = go.GetComponent<LastActivity> ();
				bool active = Time.time - lastActivity.lastActivity < activityThreshold;
				activePlayers.Add (active);
				if (active) {
					++totalActive;
				}
			}

			for (int i = 0; i < activityIndicators.Length; i++) {
				if (i < activePlayers.Count && activePlayers [i]) {
					activityIndicators [i].color = activityColor;
				} else {
					activityIndicators [i].color = Color.gray;
				}
			}

			if (totalFunky == 0) {
				return;
			}
				
			float distance = 0;
			int numDistances = 0;
			for (int n = 0; n < funkyPlayers.Count; n++) {
				if (funkyPlayers[n] != null && activePlayers [n]) {
					for (int m = 0; m < n; m++) {
						if (funkyPlayers[m] != null && activePlayers [m]) {
							distance += (funkyPlayers [n].transform.position - funkyPlayers [m].transform.position).magnitude;
							numDistances += 1;
						}
					}
				}
			}

			float funkIncrement = 100;
			if (numDistances > 0) {
				float avgDistance = distance / numDistances;
				funkIncrement += (maxDistance - avgDistance) * totalActive *
					(1f + totalFunky / totalActive) *
					(1f + Mathf.Max(0, totalFunky - 1) / totalActive) *
				    (1f + Mathf.Max(0, totalFunky - 2) / totalActive) *
				    (1f + Mathf.Max(0, totalFunky - 3) / totalActive);
			}

			funkMeter += Time.deltaTime * funkIncrement;

			if (funkMeter >= maxFunk) {
				endOfGame.gameObject.SetActive (true);
				finalText.text = timerText.text;
				finalText.gameObject.SetActive (true);
				timerText.gameObject.SetActive (false);
			}
		}
	}

	void OnGUI() {
		fillerBar.fillAmount = funkMeter/maxFunk;
	}
}
