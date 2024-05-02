using Fabric;
using UnityEngine;

public class InGameAudio : ManagedComponent
{
	public enum AudioType
	{
		SFX = 0,
		Music = 1
	}

	private const string ResourceCommonHillSounds = "CommonHillSounds";

	private const string ResourceCommonHillMusic = "CommonHillMusic";

	private const string ResourceHillMusicPrefix = "HillMusic";

	private const string PathCommon = "Sounds/Hill/";

	private const string PathRascals = "Sounds/Obstacles/Rascals/";

	private const string PathCreatures = "Sounds/Obstacles/Creatures/";

	private const string PathNatives = "Sounds/Obstacles/Natives/";

	private const string BossMonsters = "Sounds/BossMonster/";

	private const string HPathCommon = "Audio_-SFX_-Common";

	private const string HPathRascals = "Audio_-SFX_-Rascals";

	private const string HPathCreatures = "Audio_-SFX_-Creatures";

	private const string HPathNatives = "Audio_-SFX_-Natives";

	private const string HPathBossMonster = "Audio_-SFX_-BossMonster";

	private const string HPathMusic = "Audio_-Music";

	private string _bossAudioName;

	private static InGameAudio Instance { get; set; }

	private HillDefinition Defintion { get; set; }

	private void OnDestroy()
	{
		Instance = null;
		UnloadAudioResources();
	}

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("Second insatnce of HillObstacle", base.gameObject);
		}
		Instance = this;
	}

	protected override bool DoInitialise()
	{
		Defintion = CurrentHill.Instance.Definition;
		LoadAudioResources();
		return true;
	}

	private void LoadAudioResources()
	{
		LoadCommonAudio();
		LoadMusic();
		LoadObstacleAudio();
		LoadBossAudio();
	}

	private void UnloadAudioResources()
	{
		if (!(FabricManager.Instance == null))
		{
			UnloadCommonAudio();
			UnloadMusic();
			UnloadObstacleAudio();
			UnloadBossAudio();
		}
	}

	private void LoadCommonAudio()
	{
		FabricManager.Instance.LoadAsset("Sounds/Hill/CommonHillSounds", "Audio_-SFX_-Common");
	}

	private void UnloadCommonAudio()
	{
		FabricManager.Instance.UnloadAsset("Audio_-SFX_-Common_CommonHillSounds");
	}

	private void LoadMusic()
	{
		FabricManager.Instance.LoadAsset("Sounds/Hill/HillMusic" + CurrentHill.Instance.ID, "Audio_-Music");
		if (GameObject.Find("HillMusic" + CurrentHill.Instance.ID) == null)
		{
			FabricManager.Instance.LoadAsset("Sounds/Hill/CommonHillMusic", "Audio_-Music");
		}
	}

	private void UnloadMusic()
	{
		if (GameObject.Find("HillMusic" + CurrentHill.Instance.ID) != null)
		{
			FabricManager.Instance.UnloadAsset("Audio_-Music_HillMusic" + CurrentHill.Instance.ID);
		}
		else
		{
			FabricManager.Instance.UnloadAsset("Audio_-SFX_-Common_CommonHillMusic");
		}
	}

	private void LoadObstacleAudio()
	{
		foreach (CastId rascalSlot in Defintion._RascalSlots)
		{
			string prefabName = "Sounds/Obstacles/Rascals/" + rascalSlot._MouldName;
			FabricManager.Instance.LoadAsset(prefabName, "Audio_-SFX_-Rascals");
		}
		foreach (CastId creatureSlot in Defintion._CreatureSlots)
		{
			string prefabName = "Sounds/Obstacles/Creatures/" + creatureSlot._MouldName;
			FabricManager.Instance.LoadAsset(prefabName, "Audio_-SFX_-Creatures");
		}
		foreach (string nativeSlot in Defintion._NativeSlots)
		{
			string prefabName = "Sounds/Obstacles/Natives/" + nativeSlot;
			FabricManager.Instance.LoadAsset(prefabName, "Audio_-SFX_-Natives");
		}
	}

	private void UnloadObstacleAudio()
	{
		foreach (CastId rascalSlot in Defintion._RascalSlots)
		{
			FabricManager.Instance.UnloadAsset("Audio_-SFX_-Rascals_" + rascalSlot._MouldName);
		}
		foreach (CastId creatureSlot in Defintion._CreatureSlots)
		{
			FabricManager.Instance.UnloadAsset("Audio_-SFX_-Creatures_" + creatureSlot._MouldName);
		}
		foreach (string nativeSlot in Defintion._NativeSlots)
		{
			FabricManager.Instance.UnloadAsset("Audio_-SFX_-Natives_" + nativeSlot);
		}
	}

	private void LoadBossAudio()
	{
		_bossAudioName = CurrentHill.Instance.Definition._BossMonsterName;
		FabricManager.Instance.LoadAsset("Sounds/BossMonster/" + _bossAudioName, "Audio_-SFX_-BossMonster");
	}

	private void UnloadBossAudio()
	{
		FabricManager.Instance.UnloadAsset("Audio_-SFX_-BossMonster_" + _bossAudioName);
	}

	public static bool CanPlayAudio(AudioType audioType)
	{
		if (audioType == AudioType.SFX && !SaveData.Instance.Progress._optionSFXOn.Set)
		{
			return false;
		}
		if (audioType == AudioType.Music && !SaveData.Instance.Progress._optionMusicOn.Set)
		{
			return false;
		}
		return true;
	}

	public static bool PostFabricEvent(Fabric.Event postedEvent, AudioType audioType = AudioType.SFX)
	{
		if (!CanPlayAudio(audioType))
		{
			return false;
		}
		return EventManager.Instance.PostEvent(postedEvent);
	}

	public static bool PostFabricEvent(string postedEventName, AudioType audioType = AudioType.SFX)
	{
		if (!CanPlayAudio(audioType))
		{
			return false;
		}
		return EventManager.Instance.PostEvent(postedEventName);
	}

	public static bool PostFabricEvent(string eventname, GameObject parent, InitialiseParameters initialiseParameters, AudioType audioType = AudioType.SFX)
	{
		if (!CanPlayAudio(audioType))
		{
			return false;
		}
		return EventManager.Instance.PostEvent(eventname, parent, initialiseParameters);
	}

	public static bool PostFabricEvent(string eventname, EventAction eventAction, object parameter, GameObject parent, InitialiseParameters initialiseParameters, AudioType audioType = AudioType.SFX)
	{
		if ((eventAction == EventAction.PlaySound || eventAction == EventAction.UnpauseSound || eventAction == EventAction.SetSwitch) && !CanPlayAudio(audioType))
		{
			return false;
		}
		return EventManager.Instance.PostEvent(eventname, eventAction, parameter, parent, initialiseParameters);
	}

	public static bool PostFabricEvent(string eventname, EventAction eventAction, object parameter = null, GameObject parent = null, AudioType audioType = AudioType.SFX)
	{
		if ((eventAction == EventAction.PlaySound || eventAction == EventAction.UnpauseSound || eventAction == EventAction.SetSwitch) && !CanPlayAudio(audioType))
		{
			return false;
		}
		return EventManager.Instance.PostEvent(eventname, eventAction, parameter, parent);
	}

	public static void SFXToggled(bool newValue)
	{
		if (!newValue)
		{
			EventManager.Instance.PostEvent("AllSFX", EventAction.SetVolume, 0f);
		}
		else
		{
			EventManager.Instance.PostEvent("AllSFX", EventAction.SetVolume, 1f);
		}
	}

	public static void MusicToggled(bool newValue)
	{
		if (!newValue)
		{
			EventManager.Instance.PostEvent("AllMusic", EventAction.PauseSound, null);
		}
		else
		{
			EventManager.Instance.PostEvent("AllMusic", EventAction.UnpauseSound, null);
		}
	}
}
