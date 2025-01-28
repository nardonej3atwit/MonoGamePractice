﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

public class Game1 : Game
{
    

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _spriteSheet;

    private Rectangle[] backgrounds;
    private int currentBackground = 0;
    
    private Rectangle[] ships;
    private int currentShip = 0;
    private Texture2D _shipSprites;
    private float _shipSpeed = 50f;
    private float _acceleration = 100f;
    private float _deceleration = 60f;
    private Vector2 _shipVelocity = Vector2.Zero;
    private Vector2 _shipPosition;

    private RenderTarget2D _renderTarget;
    private readonly Point virtualResolution = new Point(128, 256);


    

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        //_graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        backgrounds = new Rectangle[]
        { 
            new Rectangle (0,0,128,256),
            new Rectangle (128,0,128,256),
            new Rectangle (0,256,128,256),
            new Rectangle (128,256,128,256),
            new Rectangle (0,512,128,256),
            new Rectangle (128,512,128,256)
        };
         ships = new Rectangle[]
        {
            new Rectangle (8,0,8,8),
            new Rectangle (0,0,8,8),
            new Rectangle (16,0,8,8) 
            // new Rectangle (128,0,128,256),
            // new Rectangle (0,256,128,256),
            // new Rectangle (128,256,128,256),
            // new Rectangle (0,512,128,256),
            // new Rectangle (128,512,128,256)
        };

        _graphics.PreferredBackBufferWidth = 256;  
        _graphics.PreferredBackBufferHeight = 512; 
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {

            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            _renderTarget = new RenderTarget2D( GraphicsDevice, virtualResolution.X, virtualResolution.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteSheet = Content.Load<Texture2D>("SpaceShooterAssets/SpaceShooterAssetPack_BackGrounds");
            _shipSprites = Content.Load<Texture2D>("SpaceShooterAssets/SpaceShooterAssetPack_Ships");
            Console.WriteLine("Content loaded successfully!");

            Rectangle sourceShip = ships[currentShip];
            _shipPosition = new Vector2(
            (virtualResolution.X / 2 ) - (sourceShip.Width / 2),
            virtualResolution.Y - sourceShip.Height - 10
            );
    }


    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState state = Keyboard.GetState();

        Vector2 targetVelocity = Vector2.Zero;
        

        if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
        {
            targetVelocity.Y = -1;
        }
          if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
        {
            targetVelocity.Y = 1;
        }
        if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left))
        {
            targetVelocity.X = -1;
            currentShip = 1;
        }
        if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right))
        {
            targetVelocity.X = 1;
            currentShip = 2;
        }
        if(!state.IsKeyDown(Keys.A) && !state.IsKeyDown(Keys.D) && !state.IsKeyDown(Keys.Left) && !state.IsKeyDown(Keys.Right)) {
            currentShip = 0;
        }

        if(targetVelocity != Vector2.Zero){
            targetVelocity.Normalize();
        }

        _shipVelocity =Vector2.Lerp(_shipVelocity, targetVelocity * _shipSpeed, _acceleration * deltaTime);

        _shipPosition += _shipVelocity * deltaTime;


        if(targetVelocity == Vector2.Zero){
            _shipVelocity = Vector2.Lerp(_shipVelocity, Vector2.Zero, _deceleration * deltaTime);
        }
        

        Rectangle gameFrame = new Rectangle(0,0, virtualResolution.X, virtualResolution.Y);
        _shipPosition.X = Math.Clamp(_shipPosition.X, gameFrame.X, gameFrame.Right - ships[currentShip].Width);
        _shipPosition.Y = Math.Clamp(_shipPosition.Y, gameFrame.Y, gameFrame.Bottom - ships[currentShip].Height);

        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

  
        Rectangle sourceRect = backgrounds[currentBackground];
        Rectangle sourceShip = ships[currentShip];

        _spriteBatch.Draw(_spriteSheet, new Rectangle(0,0, virtualResolution.X, virtualResolution.Y), sourceRect,Color.White);
        _spriteBatch.Draw(_shipSprites, _shipPosition,sourceShip, Color.White);

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.Opaque,
            SamplerState.PointClamp,
            DepthStencilState.Default,
            RasterizerState.CullNone
        );

        float scaleX = (float)GraphicsDevice.Viewport.Width / virtualResolution.X;
        float scaleY = (float)GraphicsDevice.Viewport.Height / virtualResolution.Y;
        float scale = Math.Min(scaleX,scaleY);

        int scaledWidth  = (int)(virtualResolution.X * scale);
        int scaledHeight  = (int)(virtualResolution.Y * scale);
        int offsetX = (GraphicsDevice.Viewport.Width - scaledWidth) / 2;
        int offsetY = (GraphicsDevice.Viewport.Height - scaledHeight) / 2;

        _spriteBatch.Draw(_renderTarget, new Rectangle(offsetX,offsetY, scaledWidth, scaledHeight),Color.White);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
