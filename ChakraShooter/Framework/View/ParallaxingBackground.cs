using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ChakraShooter
{
	public class ParallaxingBackground
	{
		// The image representing the parallaxing background
		private Texture2D texture;

		// An array of positions of the parallaxing background
		private Vector2[] positions;

		// The speed which the background is moving
		private int speed;
	}

	public void Initialize(ContentManager content, String texturePath, int screenWidth, int speed)
	{
		// Load the background texture we will be using
		texture = content.Load(texturePath);

		// Set the speed of the background
		this.speed = speed;

		// If we divide the screen with the texture width then we can determine the number of tiles need.
		// We add 1 to it so that we won't have a gap in the tiling
		positions = new Vector2[screenWidth / texture.Width + 1];

		// Set the initial positions of the parallaxing background
		for (int i = 0; i < positions.Length; i++)
		{
			// We need the tiles to be side by side to create a tiling effect
			positions[i] = new Vector2(i * texture.Width, 0);
		}
	}
}
