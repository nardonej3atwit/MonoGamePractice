using System;
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

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 512;  // Adjust to your needs
        _graphics.PreferredBackBufferHeight = 768; // Match the sprite sheet height
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

        _graphics.PreferredBackBufferWidth = 512;  
        _graphics.PreferredBackBufferHeight = 768; 
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        Console.WriteLine("Loading Content...");
        try
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteSheet = Content.Load<Texture2D>("SpaceShooterAssets/SpaceShooterAssetPack_BackGrounds");
            Console.WriteLine("Content loaded successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading content: {ex.Message}");
        }
    }


    protected override void Update(GameTime gameTime)
    {
        KeyboardState state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.Right))
        {
            currentBackground = (currentBackground + 1) % backgrounds.Length;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        int screenWidth = GraphicsDevice.Viewport.Width;
        int screenHeight = GraphicsDevice.Viewport.Height;

        Rectangle sourceRect = backgrounds[currentBackground];

        for(int x =0; x < screenWidth; x += sourceRect.Width){
            for(int y=0; y < screenHeight; y += sourceRect.Height){
                 _spriteBatch.Draw(
                    _spriteSheet, 
                    new Vector2(x, y),
                    sourceRect, 
                    Color.White
                );    
            }
        }

       
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
