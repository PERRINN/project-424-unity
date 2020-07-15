//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;


namespace VehiclePhysics.UI
{

#if !VPP_LIMITED

public class ReplayPanel : MonoBehaviour
	{
	public VPReplayController replayController;

	[Header("UI")]
	public GameObject replayPanel;
	public Text timeLabel;
	public SetableSlider slider;

	[Space(5)]
	public Text playLabel;
	public string playText = ">";
	public string pauseText = "||";

	[Header("Advanced")]
    public bool enableHotKeys = false;

	public KeyCode rewindKey = KeyCode.F9;
	public KeyCode playPauseKey = KeyCode.F10;
	public KeyCode forwardKey = KeyCode.F11;
	public KeyCode recordKey = KeyCode.F12;

	[Space(5)]
	public bool manualShowAndHide = false;
	public KeyCode showHideKey = KeyCode.Backspace;

	// Private fields

	bool m_rewindPressed = false;
	bool m_forwardPressed = false;
	bool m_shouldReturnToPlay = false;
	bool m_manualShowPanel = false;


	// UI event receivers

	public bool rewindPressed
		{
		get { return m_rewindPressed; }

		set
			{
			if (m_rewindPressed != value)
				{
				ProcessFastPlaybackState(value, VPReplay.PlaybackDirection.Reverse);
				m_rewindPressed = value;
				}
			}
		}


	public bool forwardPressed
		{
		get	{ return m_forwardPressed; }

		set
			{
			if (m_forwardPressed != value)
				{
				ProcessFastPlaybackState(value, VPReplay.PlaybackDirection.Forward);
				m_forwardPressed = value;
				}
			}
		}


	public void PositionChanged ()
		{
		if (replayController != null)
			{
			if (replayController.replay.state == VPReplay.State.Playback)
				replayController.PlayPauseKey();

			replayController.replay.Jump(Mathf.RoundToInt(slider.value));
			}
		}


	public void PlayPauseClick ()
		{
		if (replayController != null)
			{
			// Pausing / restarting playback is forward always

			replayController.replay.playbackDirection = VPReplay.PlaybackDirection.Forward;
			replayController.PlayPauseKey();
			}
		}


	public void ReturnClick ()
		{
		if (replayController != null)
			replayController.ReplayKey();
		}


	// Component methods

	void OnEnable ()
		{
		if (manualShowAndHide) m_manualShowPanel = replayPanel.activeInHierarchy;
		}


	void Update ()
		{
		if (replayController == null) return;

		// Apply hot keys

		if (enableHotKeys)
			{
			if (Input.GetKeyDown(forwardKey)) forwardPressed = true;
			if (Input.GetKeyUp(forwardKey)) forwardPressed = false;

			if (Input.GetKeyDown(rewindKey)) rewindPressed = true;
			if (Input.GetKeyUp(rewindKey)) rewindPressed = false;

			if (Input.GetKeyDown(playPauseKey)) PlayPauseClick();
			if (Input.GetKeyDown(recordKey)) ReturnClick();

			if (Input.GetKeyDown(showHideKey)) m_manualShowPanel = !m_manualShowPanel;
			}

		// Apply the events

		if (replayController == null || replayController.replay == null) return;

		if (forwardPressed) replayController.ForwardKey();
		if (rewindPressed) replayController.RewindKey();

		// Update the panel

		if (replayPanel != null)
			{
			UpdatePanelVisibility();

			if (replayPanel.activeInHierarchy)
				{
				// Update slider and labels

				int maxFrame = replayController.replay.totalFrames-1;

				if (slider != null)
					{
					if (slider.maxValue != maxFrame)
						slider.SetRangeWithoutCallback(0, maxFrame);

					slider.SetValueWithoutCallback(replayController.replay.currentFrame);
					}

				if (timeLabel != null)
					timeLabel.text = replayController.replay.FormatTime(replayController.replay.currentFrame) + " / " + replayController.replay.FormatTime(maxFrame);

				if (playLabel != null)
					{
					if (replayController.replay.state == VPReplay.State.Playback || replayController.replay.state == VPReplay.State.Record || m_shouldReturnToPlay)
						playLabel.text = pauseText;
					else
						playLabel.text = playText;
					}
				}
			}
		}


	// Utility methods


	void UpdatePanelVisibility ()
		{
		// Verify whether the panel should be visible or not

		bool showPanel = manualShowAndHide ? m_manualShowPanel :
			replayController.replay.state == VPReplay.State.Playback
			|| replayController.replay.state == VPReplay.State.Paused;

		if (showPanel && !replayPanel.activeSelf)
			{
			replayPanel.SetActive(true);
			}
		else
		if (!showPanel && replayPanel.activeSelf)
			{
			replayPanel.SetActive(false);
			}
		}


	void ProcessFastPlaybackState (bool pressed, VPReplay.PlaybackDirection direction)
		{
		if (replayController == null) return;

		// If forward or rewind is pressed while playing the same direction, then set the
		// fast-playback mode until the button is released.

		if (pressed)
			{
			if (replayController.replay.state == VPReplay.State.Playback
				&& replayController.replay.playbackDirection == direction)
				{
				replayController.PlayPauseKey();
				m_shouldReturnToPlay = true;
				}
			}
		else
			{
			if (m_shouldReturnToPlay && replayController.replay.state != VPReplay.State.Playback)
				replayController.PlayPauseKey();

			m_shouldReturnToPlay = false;
			}
		}
	}

#endif
}