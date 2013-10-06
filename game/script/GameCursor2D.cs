using System;
using UnityEngine;

namespace ja2.script
{
	//! 2D game cursor implementation.
	public class GameCursor2D : MonoBehaviourEx, IGameCursor
	{
#region Constants
		private const byte CURSOR_WIDTH = 64;
		private const byte CURSOR_HEIGHT = 64; 
#endregion

#region Textures
		//! Nonmoveable.
		public Texture2D m_TextureNonMoveable;
#endregion

#region Fields
		//! Active texture.
		private Texture2D m_ActiveTexture;
		//! Active texture FPS.
		private byte m_Fps;
		//! Number of frames.
		private byte m_Frames;
		//! Actual showing frame.
		private byte m_ActualFrame;
		//! Time passed since last frame change.
		private float m_LastFrameTime;
		//! Time needed for 1 frame.
		private float m_TimeForFrame;
		//! Texture coordinates for current frame.
		private Rect m_ActualTexCoords;
		//! Dimension of 1 frame of animation.
		private Vector2 m_FrameDimension;
#endregion

#region Interface
		//! Set cursor type.
		public void SetCursorType(GameCursorType CursorType)
		{
			switch(CursorType)
			{
				case GameCursorType.Nonmoveable:
					m_ActiveTexture = m_TextureNonMoveable;
					m_Fps = 5;
					break;
				default:
					throw new ArgumentException("GameCursor2D: Bad cursor type passed " + CursorType.ToString());
			}

			SetupTexture();
		}
#endregion

#region Operations
		//! Set up the cursor texture.
		private void SetupTexture()
		{
			// Get the number of frames
			m_Frames = (byte)(m_ActiveTexture.width / CURSOR_WIDTH);
			m_ActualFrame = 0;
			m_LastFrameTime = 0;
			m_TimeForFrame = 1f / m_Fps;

			m_FrameDimension = new Vector2(1f / m_Frames, 1);
		}
#endregion

#region Messages
		void Update()
		{
			// Is animated
			if(m_Frames > 1)
			{
				// Need to calculate actual frame position. Need to handle it in
				// loop becuase if some lag happens we could be skipping some
				// frames
				m_LastFrameTime += Time.deltaTime;
				while(m_LastFrameTime > m_TimeForFrame)
				{
					m_ActualFrame = (byte)(++m_ActualFrame % (m_Frames + 1));
					m_LastFrameTime -= m_TimeForFrame;
				}
			}

			m_ActualTexCoords = new Rect(m_ActualFrame * m_FrameDimension.x, 0, m_FrameDimension.x, m_FrameDimension.y);
		}

		void OnGUI()
		{
			// Draw cursor
			GUI.DrawTextureWithTexCoords(new Rect(Input.mousePosition.x - CURSOR_WIDTH / 2, Screen.height - Input.mousePosition.y - CURSOR_HEIGHT / 2, CURSOR_WIDTH, CURSOR_HEIGHT), m_ActiveTexture, m_ActualTexCoords);
		}
#endregion
	}
} /*ja2.script*/