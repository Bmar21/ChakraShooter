﻿using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ChakraShooter.Model;
using ChakraShooter.View;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace ChakraShooter.Controller
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class ZenGame : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public ZenGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		// Image used to display the static background
		private Texture2D mainBackground;

		// Parallaxing Layers
		private ParallaxingBackground bgLayer1;
		private ParallaxingBackground bgLayer2;

		// Represents the player 
		private Player player;

		// Keyboard states used to determine key presses
		private KeyboardState currentKeyboardState;
		private KeyboardState previousKeyboardState;

		// Gamepad states used to determine button presses
		private GamePadState currentGamePadState;
		private GamePadState previousGamePadState;

		// A movement speed for the player
		private float playerMoveSpeed;

		// Enemies
		private Texture2D enemyTexture;
		private List<Enemy> enemies;

		// The rate at which the enemies appear
		private TimeSpan enemySpawnTime;
		private TimeSpan previousSpawnTime;

		// A random number generator
		private Random random;

		private Texture2D projectileTexture;
		private List<Projectile> projectiles;

        private Texture2D EarthTexture;
		private List<Earth> earth;

        private Texture2D FireTexture;
        private List<Fire> fire;

        private Texture2D WaterTexture;
        private List<Water> water;

        // The rate of fire of the player laser
        private TimeSpan fireTime;
		private TimeSpan previousFireTime;

		private Texture2D explosionTexture;
		private List<Animation> explosions;

        // The sound that is played when a laser is fired
        private SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        private SoundEffect explosionSound;

        // The music played during gameplay
        private Song gameplayMusic;

        //Number that holds the player score
        private int score;
        // The font used to display UI elements
        private SpriteFont font;

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// Initialize the player class
			player = new Player();

			// Set a constant player move speed
			playerMoveSpeed = 6.0f;

			bgLayer1 = new ParallaxingBackground();
			bgLayer2 = new ParallaxingBackground();

			// Initialize the enemies list
			enemies = new List<Enemy>();

			// Set the time keepers to zero
			previousSpawnTime = TimeSpan.Zero;

			// Used to determine how fast enemy respawns
			enemySpawnTime = TimeSpan.FromSeconds(4.0f);

			// Initialize our random number generator
			random = new Random();

			projectiles = new List<Projectile>();
            earth = new List<Earth>();
            fire = new List<Fire>();
            water = new List<Water>();



            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);

			explosions = new List<Animation>();

            //Set player's score to zero
            score = 0;

			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load the player resources
			Animation playerAnimation = new Animation();
			Texture2D playerTexture = Content.Load<Texture2D>("Animation/MasterMonkAnimation");
			playerAnimation.Initialize(playerTexture, Vector2.Zero, 180, 100, 2, 150, Color.White, 1f, true);

			Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
			player.Initialize(playerAnimation, playerPosition);

            // Load the music
            gameplayMusic = Content.Load<Song>("Sound/gameMusic");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("Sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("Sound/explosion");

            // Start the music right away
            PlayMusic(gameplayMusic);

			// Load the parallaxing background
			bgLayer1.Initialize(Content, "Texture/Prettybackground", GraphicsDevice.Viewport.Width, -1);
			bgLayer2.Initialize(Content, "Texture/ZenBackground", GraphicsDevice.Viewport.Width, -2);

			mainBackground = Content.Load<Texture2D>("Texture/ZenBackground");

			enemyTexture = Content.Load<Texture2D>("Animation/AkumaHeadAnimation");

			projectileTexture = Content.Load<Texture2D>("Texture/ElementAir");

            EarthTexture = Content.Load<Texture2D>("Texture/ElementEarth");

            FireTexture = Content.Load<Texture2D>("Texture/ElementFire");

            WaterTexture = Content.Load<Texture2D>("Texture/ElementWater");

            explosionTexture = Content.Load<Texture2D>("Animation/explosion");

            // Load the score font
            font = Content.Load<SpriteFont>("Font/gameFont");


			//TODO: use this.Content to load your game content here 
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
#endif

			// Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
			previousGamePadState = currentGamePadState;
			previousKeyboardState = currentKeyboardState;

			// Read the current state of the keyboard and gamepad and store it
			currentKeyboardState = Keyboard.GetState();
			currentGamePadState = GamePad.GetState(PlayerIndex.One);

			//Update the player
			UpdatePlayer(gameTime);

			// Update the collision
			UpdateCollision();

			// Update the projectiles

		    UpdateProjectiles();
            UpdateEarth();
            UpdateFire();
            UpdateWater();
           

            // Update the explosions
            UpdateExplosions(gameTime);

			// Update the parallaxing background
			bgLayer1.Update();
			bgLayer2.Update();

			// Update the enemies
			UpdateEnemies(gameTime);

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			// Start drawing 
			spriteBatch.Begin();

			spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

			// Draw the moving background
			bgLayer1.Draw(spriteBatch);
			bgLayer2.Draw(spriteBatch);

			// Draw the Player 
			player.Draw(spriteBatch);

			// Draw the Enemies
			for (int i = 0; i < enemies.Count; i++)
			{
				enemies[i].Draw(spriteBatch);
			}

			// Draw the Projectiles
			for (int i = 0; i < projectiles.Count; i++)
			{
				projectiles[i].Draw(spriteBatch);
			}

            // Draw the Projectiles
            for (int i = 0; i < earth.Count; i++)
            {
                earth[i].Draw(spriteBatch);
            }

            // Draw the Projectiles
            for (int i = 0; i < fire.Count; i++)
            {
                fire[i].Draw(spriteBatch);
            }

            // Draw the Projectiles
            for (int i = 0; i < water.Count; i++)
            {
                water[i].Draw(spriteBatch);
            }

            // Draw the explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Draw(spriteBatch);
            }

            // Draw the score
            spriteBatch.DrawString(font, "score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            // Draw the player health
            spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);


			// Stop drawing 
			spriteBatch.End();


			//TODO: Add your drawing code here

			base.Draw(gameTime);
		}

		private void UpdatePlayer(GameTime gameTime)
		{
			player.Update(gameTime);

			// Get Thumbstick Controls
			player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
			player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

			// Use the Keyboard / Dpad
			if (currentKeyboardState.IsKeyDown(Keys.Left) || currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				player.Position.X -= playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.Right) || currentGamePadState.DPad.Right == ButtonState.Pressed)
			{
				player.Position.X += playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Up == ButtonState.Pressed)
			{
				player.Position.Y -= playerMoveSpeed;
			}
			if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Down == ButtonState.Pressed)
			{
				player.Position.Y += playerMoveSpeed;
			}

			// Make sure that the player does not go out of bounds
			player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
			player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

			// Fire only every interval we set as the fireTime
			if (currentKeyboardState.IsKeyDown(Keys.A))
			{
				// Reset our current time
				previousFireTime = gameTime.TotalGameTime;

				// Add the projectile, but add it to the front and center of the player
				AddProjectile(player.Position + new Vector2(player.Width / 2, 0));

                // Play the laser sound
                laserSound.Play();
			}

            // Fire only every interval we set as the fireTime
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddEarth(player.Position + new Vector2(player.Width / 2, 0));

                // Play the laser sound
                laserSound.Play();
            }

            // Fire only every interval we set as the fireTime
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddFire(player.Position + new Vector2(player.Width / 2, 0));

                // Play the laser sound
                laserSound.Play();
            }

            // Fire only every interval we set as the fireTime
            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddWater(player.Position + new Vector2(player.Width / 2, 0));

                // Play the laser sound
                laserSound.Play();
            }


            // reset score if player health goes to zero
            if (player.Health <= 0)
            {
                player.Health = 100;
                score = 0;
            }

		}

		private void AddEnemy()
		{
			// Create the animation object
			Animation enemyAnimation = new Animation();

			// Initialize the animation with the correct animation information
			enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 50, 65, 2, 60, Color.White, 1f, true);

			// Randomly generate the position of the enemy
			Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

			// Create an enemy
			Enemy enemy = new Enemy();

			// Initialize the enemy
			enemy.Initialize(enemyAnimation, position);

			// Add the enemy to the active enemies list
			enemies.Add(enemy);
		}

		private void UpdateEnemies(GameTime gameTime)
		{
			// Spawn a new enemy enemy every 1.5 seconds
			if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
			{
				previousSpawnTime = gameTime.TotalGameTime;

				// Add an Enemy
				AddEnemy();
			}

			// Update the Enemies
			for (int i = enemies.Count - 1; i >= 0; i--)
			{
				enemies[i].Update(gameTime);

				if (enemies[i].Active == false)
				{
                    // If not active and health <= 0
                    if (enemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(enemies[i].Position);
                        // Play the explosion sound
                        explosionSound.Play();
                        //Add to the player's score
                        score += enemies[i].ScoreValue;
                    }

					enemies.RemoveAt(i);

				}
			}

		}

		private void UpdateCollision()
		{
			// Use the Rectangle's built-in intersect function to 
			// determine if two objects are overlapping
			Rectangle rectangle1;
			Rectangle rectangle2;

			// Only create the rectangle once for the player
			rectangle1 = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.Width, player.Height);

			// Do the collision between the player and the enemies
			for (int i = 0; i < enemies.Count; i++)
			{
				rectangle2 = new Rectangle((int)enemies[i].Position.X, (int)enemies[i].Position.Y, enemies[i].Width, enemies[i].Height);

				// Determine if the two objects collided with each other
				if (rectangle1.Intersects(rectangle2))
				{
					// Subtract the health from the player based on
					// the enemy damage
					player.Health -= enemies[i].Damage;

					// Since the enemy collided with the player
					// destroy it
					enemies[i].Health = 0;

					// If the player health is less than zero we died
					if (player.Health <= 0)
					{
						player.Active = false;
					}
				}
			}

            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)projectiles[i].Position.X - projectiles[i].Width / 2, (int) projectiles[i].Position.Y - 
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2, (int) enemies[j].Position.Y - enemies[j].Height / 2, enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= projectiles[i].Damage;
                        projectiles[i].Active = false;
                    }
                }
            }

            for (int i = 0; i < earth.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)earth[i].Position.X - earth[i].Width / 2, (int)earth[i].Position.Y -
                    earth[i].Height / 2, earth[i].Width, earth[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2, (int)enemies[j].Position.Y - enemies[j].Height / 2, enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= earth[i].Damage;
                        earth[i].Active = false;
                    }
                }
            }

            for (int i = 0; i < fire.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)fire[i].Position.X - fire[i].Width / 2, (int)fire[i].Position.Y -
                    fire[i].Height / 2, fire[i].Width, fire[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2, (int)enemies[j].Position.Y - enemies[j].Height / 2, enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= fire[i].Damage;
                        fire[i].Active = false;
                    }
                }
            }

            for (int i = 0; i < water.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)water[i].Position.X - water[i].Width / 2, (int)water[i].Position.Y -
                    water[i].Height / 2, water[i].Width, water[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2, (int)enemies[j].Position.Y - enemies[j].Height / 2, enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= water[i].Damage;
                        water[i].Active = false;
                    }
                }
            }

        }
        
        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile(); 
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position); 
            projectiles.Add(projectile);
        }

        private void AddEarth(Vector2 position)
        {
            Earth earthElement = new Earth();
            earthElement.Initialize(GraphicsDevice.Viewport, EarthTexture, position);
            earth.Add(earthElement);
        }

        private void AddFire(Vector2 position)
        {
            Fire fireElement = new Fire();
            fireElement.Initialize(GraphicsDevice.Viewport, FireTexture, position);
            fire.Add(fireElement);
        }

        private void AddWater(Vector2 position)
        {
            Water waterElement = new Water();
            waterElement.Initialize(GraphicsDevice.Viewport, WaterTexture, position);
            water.Add(waterElement);
        }


            private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--) 
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                } 
            }
        }

        private void UpdateEarth()
        {
            // Update the Projectiles
            for (int i = earth.Count - 1; i >= 0; i--)
            {
                earth[i].Update();

                if (earth[i].Active == false)
                {
                    earth.RemoveAt(i);
                }
            }
        }

        private void UpdateFire()
        {
            // Update the Projectiles
            for (int i = fire.Count - 1; i >= 0; i--)
            {
                fire[i].Update();

                if (fire[i].Active == false)
                {
                    fire.RemoveAt(i);
                }
            }
        }

        private void UpdateWater()
        {
            // Update the Projectiles
            for (int i = water.Count - 1; i >= 0; i--)
            {
                water[i].Update();

                if (water[i].Active == false)
                {
                    water.RemoveAt(i);
                }
            }
        }


        private void AddExplosion(Vector2 position)
        {
          Animation explosion = new Animation();
          explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
          explosions.Add(explosion);
        }

        private void UpdateExplosions(GameTime gameTime)
            {
                for (int i = explosions.Count - 1; i >= 0; i--)
                {
                    explosions[i].Update(gameTime);
                    if (explosions[i].Active == false)
                    {
                         explosions.RemoveAt(i);
                    }
                }
            }
        
        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { } //No Exception is handled so it is an empty/anonymous exception
        }

	}
}
