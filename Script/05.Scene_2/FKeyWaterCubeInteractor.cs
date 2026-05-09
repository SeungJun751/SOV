using System.Linq;
using UnityEngine;

public class FKeyWaterCubeInteractor : MonoBehaviour
{
	[Header("설정")]
	public float detectRange = 10f; 

	[Header("참조")]
	public Transform player; 
	public MelodyPuzzleManager puzzleManager; 
	
	[Header("UI")]
	public GameObject fKeyUI; 

	void Start()
	{
		if (player == null)
		{
			var found = GameObject.FindWithTag("Player");
			if (found != null) player = found.transform;
		}
		if (puzzleManager == null)
		{
			puzzleManager = FindFirstObjectByType<MelodyPuzzleManager>();
		}
		if (fKeyUI == null)
		{
			GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
			foreach (var obj in allObjects)
			{
				if (obj.name.Contains("FKey") || obj.name.Contains("F키") || obj.name.Contains("InteractionPanel"))
				{
					fKeyUI = obj;
					fKeyUI.SetActive(false);
					break;
				}
			}
		}
	}

	void Update()
	{
		if (player == null || puzzleManager == null) return;

		bool isNearWater = IsNearAnyWaterPit();
		
		if (fKeyUI != null)
		{
			fKeyUI.SetActive(isNearWater);
		}

		if (Input.GetKeyDown(KeyCode.F) && isNearWater)
		{
			TryPlayNearestCubeAtNearestWater();
		}
	}
	
	private bool IsNearAnyWaterPit()
	{
		if (puzzleManager == null || puzzleManager.waterPits == null) return false;
		
		foreach (var pit in puzzleManager.waterPits)
		{
			if (pit == null) continue;
			float distance = Vector3.Distance(player.position, pit.transform.position);
			if (distance <= detectRange)
			{
				return true;
			}
		}
		return false;
	}

	private void TryPlayNearestCubeAtNearestWater()
	{
		var pits = puzzleManager.waterPits;
		if (pits == null || pits.Length == 0) 
		{
			return;
		}


		WaterSound nearestPit = null;
		float bestPitDist = float.MaxValue;
		foreach (var pit in pits)
		{
			if (pit == null) 
			{
				continue;
			}
			
			float d = Vector3.Distance(player.position, pit.transform.position);
			
			if (d <= detectRange && d < bestPitDist)
			{
				bestPitDist = d;
				nearestPit = pit;
			}
		}
		
		if (nearestPit == null) 
		{
			return;
		}

		int noteIndex = nearestPit.GetCurrentNoteIndex();

		if (nearestPit.audioSource != null && nearestPit.soundClips != null && nearestPit.soundClips.Length > noteIndex && nearestPit.soundClips[noteIndex] != null)
		{
			nearestPit.audioSource.clip = nearestPit.soundClips[noteIndex];
			nearestPit.audioSource.Play();
		}
		else
		{
			Debug.LogError($"물구덩이 오디오 재생 실패. AudioSource: {nearestPit.audioSource != null}, 클립: {nearestPit.soundClips?[noteIndex] != null}");
		}
	}
}


