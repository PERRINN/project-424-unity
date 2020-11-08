#if false

	public enum RecordMode { New, Append, Restart, FromCurrentPos, PlayToEndThenAppend };
	public enum PlayMode { FromBegin, FromEnd, FromCurrentPos };
	public enum PauseMode { AtCurrentPos, AtBegin, AtEnd };
	public enum QuitMode { AtCurrentPos, AtBegin, AtEnd };
	public enum TrimMode { FromBegin, ToEnd };


	// Record
	//
	//	New						Discard current replay, if any, and begin recording a new one
	//	Append                  Continue recording at the end of current replay
	//	Restart                 Go to the beginning of the current replay, then record a new one
	//	FromCurrentPos          Continue recording from the current frame in the replay
	//	PlayToEndThenAppend		Play from the current frame until the end, then continue recording

	public void Record (RecordMode recordMode = RecordMode.New) { }


	// Play
	//
	//	FromBegin		Plays current replay from the beginning
	//	FromEnd			Plays current replay from the end in reverse
	//	FromCurrentPos	Plays current replay from the actual position (currentFrame)

	public void Play (PlayMode playMode = PlayMode.FromBegin) { }

	// Pause
	//
	//	AtCurrentPos	Sets pause right in the actual position in the replay (currentFrame)
	//	AtBegin			Sets pause at the beginning of the replay
	//	AtEnd			Sets pause at the end of the replay

	public void Pause (PauseMode pauseMode = PauseMode.AtCurrentPos) { }


	// Quit
	//
	//	AtCurrentPos	Quits replay launching the vehicle from the current frame
	//	AtBegin			Quits replay launching the vehicle from the first frame
	//	AtEnd			Quits replay launching the vehicle from the last frame

	public void Quit (QuitMode quitMode = QuitMode.AtCurrentPos) { }


	// Jump

	public void Jump (int frame) { }


	public void Skip (int deltaFrames) { }


	// Trim frames based on the current position

	public void Trim (TrimMode trimMode) { }


	// Remove the first n frames, but limited to the current frame.
	// This will be useful for limited replays until we add option to use a circular buffer.

	public void Drop (int dropFrames) { }


	// Load and save replays

	public VPReplayAsset SaveReplayToAsset () { }
	public void LoadReplayFromAsset (VPReplayAsset replayAsset) { }
	public void SaveReplayToAssetFile (string assetFileName) { }
	public void LoadReplayFromAssetFile (string assetFileName) { }
	public void SaveReplayToFile (string fileName) { }
	public void LoadReplayFromFile (string fileName) { }
	public void UnloadReplay () { }


	// Time to/from frames conversion and formatting


	public int TimeToFrames (float time) { }
	public float FramesToTime (int frames) { }
	public string FormatTime (int frames) { }
	public string FormatFrames (int frames) { }
	public string FormatTimeAndFrames (int frames) { }


#endif