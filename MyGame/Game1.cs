using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame;




public class Game1 : Game
{
    

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _spriteSheet;

    private Rectangle [] projectiles;

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

    private List<Projectile> _activeProjectiles;
    private Texture2D _projectileSprites;
    private float _shootCooldown = 0.2f;
    private float _currentShootTimer = 0f;

    private List<Enemy> _activeEnemies;
    private Texture2D _enemySprites;
    private Rectangle[] enemies;
    private float _enemySpawnTimer = 0f;
    private float _enemySpawnInterval = 2f;

    private Texture2D _miscSprites;
    private Rectangle[] explosions;
    private bool _gameOver = false;


    public class Projectile{
        public Vector2 Position;
        public Vector2 Velocity;
        public Rectangle SourceRect;

        public Projectile( Vector2 position, Vector2 velocity, Rectangle sourceRect){
            Position = position;
            Velocity = velocity;
            SourceRect = sourceRect;
        }
        public void Update(float deltaTime){
            Position += Velocity * deltaTime; 
        }
    }
    
    public class Enemy{
        public Vector2 Position;
        public Vector2 Velocity;
        public Rectangle[] AnimationFrames;
        public float Lifetime;
        public float FrameTimer;
        public float FrameDuration;
        public int CurrentFrame;

        public Enemy(Vector2 position, Vector2 velocity, Rectangle[] animationFrames, float lifetime = 0f, float frameDuration = 0.2f){
            Position = position;
            Velocity = velocity;
            AnimationFrames = animationFrames;
            Lifetime = lifetime;
            CurrentFrame = 0;
            FrameTimer = 0f;
            FrameDuration = frameDuration;
        }
        public void Update(float deltaTime){
            Position += Velocity * deltaTime;
            if(Lifetime >0) {
                Lifetime -= deltaTime;
                FrameTimer += deltaTime;
                if(FrameTimer >= FrameDuration){
                    FrameTimer -= FrameDuration;
                    CurrentFrame++;
                    if(CurrentFrame >= AnimationFrames.Length){
                        CurrentFrame = AnimationFrames.Length - 1;
                    }
                }
            }
        }
        public Rectangle GetCurrentFrame(){
            return AnimationFrames[CurrentFrame];
        }
    }

    private bool checkCollision(Rectangle rect1,Vector2 pos1, Rectangle rect2, Vector2 pos2){
        Rectangle box1 = new Rectangle((int) pos1.X, (int)pos1.Y, rect1.Width, rect1.Height);
        Rectangle box2 = new Rectangle((int) pos2.X, (int)pos2.Y, rect2.Width, rect2.Height);
        return box1.Intersects(box2);
    }
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
            new Rectangle (0,0,6,8),
            new Rectangle (16,0,8,8) 
        };

        projectiles = new Rectangle[]{
            new Rectangle (16,0,6,6),
            new Rectangle (0,16,6,6)
        };
        explosions = new Rectangle[]{
            new Rectangle(0,16,8,8),
            new Rectangle(16,24,8,8),
            new Rectangle(16,8,8,8),
            new Rectangle(8,32,8,8)
        };
        enemies = new Rectangle[]
        {
            new Rectangle (48,0,8,8),
            new Rectangle (56,0,8,8),
            new Rectangle (64,0,8,8),
            new Rectangle (72,0,8,8),
            new Rectangle (80,0,8,8),
            new Rectangle (88,0,8,8) 
        };


        _activeEnemies = new List<Enemy>();
        _activeProjectiles = new List<Projectile>();
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
            _enemySprites = Content.Load<Texture2D>("SpaceShooterAssets/SpaceShooterAssetPack_Ships");
            _projectileSprites = Content.Load<Texture2D>("SpaceShooterAssets/SpaceShooterAssetPack_Projectiles");
            _miscSprites = Content.Load<Texture2D>("SpaceShooterAssets/SpaceShooterAssetPack_Miscellaneous");
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
        

        if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up)) targetVelocity.Y = -1;
        if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))targetVelocity.Y = 1;
        if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left)){targetVelocity.X = -1; currentShip = 1;}
        if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right)){targetVelocity.X = 1;currentShip = 2;}

        if(!state.IsKeyDown(Keys.A) && !state.IsKeyDown(Keys.D) && !state.IsKeyDown(Keys.Left) && !state.IsKeyDown(Keys.Right)) {currentShip = 0;}

        if(targetVelocity != Vector2.Zero)targetVelocity.Normalize();

        _shipVelocity =Vector2.Lerp(_shipVelocity, targetVelocity * _shipSpeed, _acceleration * deltaTime);
        _shipPosition += _shipVelocity * deltaTime;

        if(targetVelocity == Vector2.Zero)_shipVelocity = Vector2.Lerp(_shipVelocity, Vector2.Zero, _deceleration * deltaTime);
        

        Rectangle gameFrame = new Rectangle(0,0, virtualResolution.X, virtualResolution.Y);
        _shipPosition.X = Math.Clamp(_shipPosition.X, gameFrame.X, gameFrame.Right - ships[currentShip].Width);
        _shipPosition.Y = Math.Clamp(_shipPosition.Y, gameFrame.Y, gameFrame.Bottom - ships[currentShip].Height);


        //projectiles portion of Update
        _currentShootTimer -= deltaTime;
        if(state.IsKeyDown(Keys.Space) && _currentShootTimer <= 0f)
        {
            _currentShootTimer = _shootCooldown;
            Rectangle projectileSource = projectiles[0];
            Vector2 projectileVelocity = new Vector2(0, -100);
            Vector2 projectilePosition = new Vector2(
                _shipPosition.X + (ships[currentShip].Width / 2) - (projectileSource.Width/2),
                _shipPosition.Y  - projectileSource.Height 
            );
            _activeProjectiles.Add(new Projectile(projectilePosition, projectileVelocity, projectileSource));
        }
        for(int i = _activeProjectiles.Count - 1; i >= 0; i--){
            _activeProjectiles[i].Update(deltaTime);
            if(_activeProjectiles[i].Position.Y < 0)
            {
                _activeProjectiles.RemoveAt(i);
                continue;
            }
            for(int j = _activeEnemies.Count -1; j >= 0; j--){
                if(checkCollision(_activeProjectiles[i].SourceRect, _activeProjectiles[i].Position,_activeEnemies[j].GetCurrentFrame(), _activeEnemies[j].Position))
                {
                _activeEnemies[j] = new Enemy(_activeEnemies[j].Position, Vector2.Zero, explosions, 1f,0.2f);
                _activeProjectiles.RemoveAt(i);
                break;
                }
            } 
        }


        //enemies portion of the update method
        _enemySpawnTimer -= deltaTime;
        if(_enemySpawnTimer <= 0f){
            _enemySpawnTimer = _enemySpawnInterval;
            Rectangle enemySource = enemies[0];
            Vector2 enemyPoistion = new Vector2( Random.Shared.Next(0, virtualResolution.X - enemySource.Width), -enemySource.Height);
            Vector2 enemyVelocity = new Vector2(0,50);
                        _activeEnemies.Add(new Enemy(enemyPoistion, enemyVelocity, enemies, 0f));
        }

        for(int i =_activeEnemies.Count - 1; i >= 0; i--){
            _activeEnemies[i].Update(deltaTime);
            if(_activeEnemies[i].Position.Y > virtualResolution.Y) _activeEnemies.RemoveAt(i);
        }

        for(int i = _activeEnemies.Count -1; i>=0; i--){
            _activeEnemies[i].Update(deltaTime);

            if(_activeEnemies[i].Position.Y > virtualResolution.Y){
                _activeEnemies.RemoveAt(i);
                continue;
            }
            if(_activeEnemies[i].Lifetime <= 0 && _activeEnemies[i].Lifetime != 0){
                _activeEnemies.RemoveAt(i);
            }
            if(checkCollision(_activeEnemies[i].GetCurrentFrame(), _activeEnemies[i].Position, ships[currentShip], _shipPosition)){
                _activeEnemies[i] = new Enemy(_activeEnemies[i].Position, Vector2.Zero, explosions, 1f);
                _gameOver = true;
            }

        }

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
        if(!_gameOver)_spriteBatch.Draw(_shipSprites, _shipPosition,sourceShip, Color.White);
    
        foreach(var projectiles in _activeProjectiles)
        {
            _spriteBatch.Draw(_projectileSprites, projectiles.Position, projectiles.SourceRect,Color.White);
        }
        foreach(var enemy in _activeEnemies)
        {
            Rectangle frame = enemy.GetCurrentFrame();
            if(enemy.Lifetime > 0){
                _spriteBatch.Draw(_projectileSprites, enemy.Position, frame,Color.White);
            }
            else{_spriteBatch.Draw(_enemySprites, enemy.Position, frame,Color.White);}
            
            
        }

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
